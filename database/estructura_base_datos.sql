IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'db_commerce')
BEGIN
    CREATE DATABASE db_commerce;
END
GO

USE db_commerce;
GO

-- Limpieza preventiva de procedimientos existentes
IF OBJECT_ID('dbo.sp_create_commerce', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_create_commerce;
GO

IF OBJECT_ID('dbo.sp_procesar_comercios_por_fecha', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_procesar_comercios_por_fecha;
GO

IF OBJECT_ID('dbo.sp_obtener_comercios_cuarentena', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_obtener_comercios_cuarentena;
GO

IF OBJECT_ID('dbo.sp_obtener_comercios', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_obtener_comercios;
GO

-- 1. Tabla principal de comercios (estructura idéntica a las columnas del archivo CSV)
IF OBJECT_ID('dbo.commerce', 'U') IS NOT NULL
    DROP TABLE dbo.commerce;
GO

CREATE TABLE dbo.commerce (
    id INT IDENTITY(1,1) PRIMARY KEY,
    pc_processdate VARCHAR(20) NOT NULL,
    pc_codcom VARCHAR(20) NULL,
    pc_nomcomred VARCHAR(150) NULL,
    pc_numdoc VARCHAR(20) NULL,
    pc_tipdoc VARCHAR(10) NULL,
    pc_estado VARCHAR(20) NULL,
    fecha_registro DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- 2. Tabla para almacenar los registros en cuarentena
IF OBJECT_ID('dbo.commerce_quarantine', 'U') IS NOT NULL
    DROP TABLE dbo.commerce_quarantine;
GO

CREATE TABLE dbo.commerce_quarantine (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_comercio_origen INT NULL,
    pc_processdate VARCHAR(20) NOT NULL,
    pc_codcom VARCHAR(20) NULL,
    pc_nomcomred VARCHAR(150) NULL,
    pc_numdoc VARCHAR(20) NULL,
    pc_tipdoc VARCHAR(10) NULL,
    pc_estado VARCHAR(20) NULL,
    fecha_cuarentena DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Requerimiento del enunciado: Modificar la tabla commerce_quarantine agregando la columna 'motivo'
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = N'motivo' AND Object_ID = OBJECT_ID(N'dbo.commerce_quarantine')
)
BEGIN
    ALTER TABLE dbo.commerce_quarantine
    ADD motivo VARCHAR(250) NOT NULL DEFAULT '';
END
GO

-- 3. Procedimiento para registrar un comercio individual (sp_create_commerce)
CREATE PROCEDURE dbo.sp_create_commerce
    @pc_processdate VARCHAR(20),
    @pc_codcom VARCHAR(20) = NULL,
    @pc_nomcomred VARCHAR(150) = NULL,
    @pc_numdoc VARCHAR(20) = NULL,
    @pc_tipdoc VARCHAR(10) = NULL,
    @pc_estado VARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.commerce (
        pc_processdate,
        pc_codcom,
        pc_nomcomred,
        pc_numdoc,
        pc_tipdoc,
        pc_estado,
        fecha_registro
    )
    VALUES (
        LTRIM(RTRIM(@pc_processdate)),
        NULLIF(LTRIM(RTRIM(@pc_codcom)), ''),
        NULLIF(LTRIM(RTRIM(@pc_nomcomred)), ''),
        NULLIF(LTRIM(RTRIM(@pc_numdoc)), ''),
        NULLIF(LTRIM(RTRIM(@pc_tipdoc)), ''),
        NULLIF(LTRIM(RTRIM(@pc_estado)), ''),
        GETDATE()
    );

    SELECT SCOPE_IDENTITY() AS id_insertado;
END;
GO

-- 4. Procedimiento para procesar comercios por fecha y mover observados a cuarentena
CREATE PROCEDURE dbo.sp_procesar_comercios_por_fecha
    @pc_processdate VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    -- Tabla temporal para almacenar los registros que no cumplen las condiciones
    CREATE TABLE #registros_con_error (
        id INT,
        pc_processdate VARCHAR(20),
        pc_codcom VARCHAR(20),
        pc_nomcomred VARCHAR(150),
        pc_numdoc VARCHAR(20),
        pc_tipdoc VARCHAR(10),
        pc_estado VARCHAR(20),
        motivo VARCHAR(250)
    );

    -- Detección de inconsistencias según las reglas del enunciado:
    -- 1. pc_nomcomred no debe estar vacío
    -- 2. pc_numdoc no debe estar vacío, ni contener letras ni caracteres especiales
    INSERT INTO #registros_con_error (
        id,
        pc_processdate,
        pc_codcom,
        pc_nomcomred,
        pc_numdoc,
        pc_tipdoc,
        pc_estado,
        motivo
    )
    SELECT 
        c.id,
        c.pc_processdate,
        c.pc_codcom,
        c.pc_nomcomred,
        c.pc_numdoc,
        c.pc_tipdoc,
        c.pc_estado,
        CASE
            -- Caso 1: Ambos campos vacíos
            WHEN (c.pc_nomcomred IS NULL OR LTRIM(RTRIM(c.pc_nomcomred)) = '')
                 AND (c.pc_numdoc IS NULL OR LTRIM(RTRIM(c.pc_numdoc)) = '')
                THEN 'El nombre del comercio (nomcomred) se encuentra vacio; El número (numdoc) se encuentra vacio'

            -- Caso 2: Nombre vacío y documento con letras o caracteres especiales
            WHEN (c.pc_nomcomred IS NULL OR LTRIM(RTRIM(c.pc_nomcomred)) = '')
                 AND (c.pc_numdoc LIKE '%[^0-9]%')
                THEN 'El nombre del comercio (nomcomred) se encuentra vacio; El número (numdoc) contiene letras o caracteres especiales'

            -- Caso 3: Solo nombre vacío
            WHEN (c.pc_nomcomred IS NULL OR LTRIM(RTRIM(c.pc_nomcomred)) = '')
                THEN 'El nombre del comercio (nomcomred) se encuentra vacio'

            -- Caso 4: Solo documento vacío
            WHEN (c.pc_numdoc IS NULL OR LTRIM(RTRIM(c.pc_numdoc)) = '')
                THEN 'El número de documento (numdoc) se encuentra vacio'

            -- Caso 5: Documento con letras o caracteres especiales
            WHEN (c.pc_numdoc LIKE '%[^0-9]%')
                THEN 'El número (numdoc) contiene letras o caracteres especiales'

            ELSE 'Registro no válido'
        END AS motivo
    FROM dbo.commerce c
    WHERE c.pc_processdate = LTRIM(RTRIM(@pc_processdate))
      AND (
          c.pc_nomcomred IS NULL 
          OR LTRIM(RTRIM(c.pc_nomcomred)) = ''
          OR c.pc_numdoc IS NULL 
          OR LTRIM(RTRIM(c.pc_numdoc)) = ''
          OR c.pc_numdoc LIKE '%[^0-9]%'
      );

    DECLARE @cantidad_cuarentena INT = 0;

    -- Transacción atómica: inserción en cuarentena y eliminación de la tabla principal
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO dbo.commerce_quarantine (
            id_comercio_origen,
            pc_processdate,
            pc_codcom,
            pc_nomcomred,
            pc_numdoc,
            pc_tipdoc,
            pc_estado,
            fecha_cuarentena,
            motivo
        )
        SELECT 
            id,
            pc_processdate,
            pc_codcom,
            pc_nomcomred,
            pc_numdoc,
            pc_tipdoc,
            pc_estado,
            GETDATE(),
            motivo
        FROM #registros_con_error;

        SET @cantidad_cuarentena = @@ROWCOUNT;

        -- Eliminar de la tabla commerce los registros trasladados a cuarentena
        DELETE c
        FROM dbo.commerce c
        INNER JOIN #registros_con_error e ON c.id = e.id;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DROP TABLE #registros_con_error;
        THROW;
    END CATCH;

    DROP TABLE #registros_con_error;

    -- Retorna la cantidad de registros insertados en la tabla commerce_quarantine
    SELECT @cantidad_cuarentena AS registros_en_cuarentena;
END;
GO

-- 5. Procedimiento para listar los comercios en cuarentena
CREATE PROCEDURE dbo.sp_obtener_comercios_cuarentena
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        id,
        id_comercio_origen,
        pc_processdate,
        pc_codcom,
        pc_nomcomred,
        pc_numdoc,
        pc_tipdoc,
        pc_estado,
        motivo,
        fecha_cuarentena
    FROM dbo.commerce_quarantine
    ORDER BY fecha_cuarentena DESC, id DESC;
END;
GO

-- 6. Procedimiento para listar los comercios vigentes
CREATE PROCEDURE dbo.sp_obtener_comercios
    @pc_processdate VARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        id,
        pc_processdate,
        pc_codcom,
        pc_nomcomred,
        pc_numdoc,
        pc_tipdoc,
        pc_estado,
        fecha_registro
    FROM dbo.commerce
    WHERE (@pc_processdate IS NULL OR pc_processdate = LTRIM(RTRIM(@pc_processdate)))
    ORDER BY fecha_registro DESC, id DESC;
END;
GO
