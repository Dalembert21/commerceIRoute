IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'db_commerce')
BEGIN
    CREATE DATABASE db_commerce;
END
GO

USE db_commerce;
GO

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

-- Tabla principal de comercios
IF OBJECT_ID('dbo.commerce', 'U') IS NOT NULL
    DROP TABLE dbo.commerce;
GO

CREATE TABLE dbo.commerce (
    id INT IDENTITY(1,1) PRIMARY KEY,
    fecha_proceso VARCHAR(20) NOT NULL,
    codigo_comercio VARCHAR(20) NULL,
    nombre_comercial VARCHAR(150) NULL,
    numero_documento VARCHAR(20) NULL,
    tipo_documento VARCHAR(10) NULL,
    estado VARCHAR(20) NULL,
    fecha_registro DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Tabla para almacenar los registros en cuarentena
IF OBJECT_ID('dbo.commerce_quarantine', 'U') IS NOT NULL
    DROP TABLE dbo.commerce_quarantine;
GO

CREATE TABLE dbo.commerce_quarantine (
    id INT IDENTITY(1,1) PRIMARY KEY,
    id_comercio_origen INT NULL,
    fecha_proceso VARCHAR(20) NOT NULL,
    codigo_comercio VARCHAR(20) NULL,
    nombre_comercial VARCHAR(150) NULL,
    numero_documento VARCHAR(20) NULL,
    tipo_documento VARCHAR(10) NULL,
    estado VARCHAR(20) NULL,
    fecha_cuarentena DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Columna motivo en la tabla de cuarentena
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = N'motivo' AND Object_ID = OBJECT_ID(N'dbo.commerce_quarantine')
)
BEGIN
    ALTER TABLE dbo.commerce_quarantine
    ADD motivo VARCHAR(250) NOT NULL DEFAULT '';
END
GO

-- Procedimiento para registrar un comercio
CREATE PROCEDURE dbo.sp_create_commerce
    @fecha_proceso VARCHAR(20),
    @codigo_comercio VARCHAR(20) = NULL,
    @nombre_comercial VARCHAR(150) = NULL,
    @numero_documento VARCHAR(20) = NULL,
    @tipo_documento VARCHAR(10) = NULL,
    @estado VARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.commerce (
        fecha_proceso,
        codigo_comercio,
        nombre_comercial,
        numero_documento,
        tipo_documento,
        estado,
        fecha_registro
    )
    VALUES (
        LTRIM(RTRIM(@fecha_proceso)),
        NULLIF(LTRIM(RTRIM(@codigo_comercio)), ''),
        NULLIF(LTRIM(RTRIM(@nombre_comercial)), ''),
        NULLIF(LTRIM(RTRIM(@numero_documento)), ''),
        NULLIF(LTRIM(RTRIM(@tipo_documento)), ''),
        NULLIF(LTRIM(RTRIM(@estado)), ''),
        GETDATE()
    );

    SELECT SCOPE_IDENTITY() AS id_insertado;
END;
GO

-- Procedimiento para procesar comercios por fecha y mover observados a cuarentena
CREATE PROCEDURE dbo.sp_procesar_comercios_por_fecha
    @fecha_proceso VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #registros_con_error (
        id INT,
        fecha_proceso VARCHAR(20),
        codigo_comercio VARCHAR(20),
        nombre_comercial VARCHAR(150),
        numero_documento VARCHAR(20),
        tipo_documento VARCHAR(10),
        estado VARCHAR(20),
        motivo VARCHAR(250)
    );

    INSERT INTO #registros_con_error (
        id,
        fecha_proceso,
        codigo_comercio,
        nombre_comercial,
        numero_documento,
        tipo_documento,
        estado,
        motivo
    )
    SELECT 
        c.id,
        c.fecha_proceso,
        c.codigo_comercio,
        c.nombre_comercial,
        c.numero_documento,
        c.tipo_documento,
        c.estado,
        CASE
            WHEN (c.nombre_comercial IS NULL OR LTRIM(RTRIM(c.nombre_comercial)) = '')
                 AND (c.numero_documento IS NULL OR LTRIM(RTRIM(c.numero_documento)) = '')
                THEN 'El nombre del comercio (nomcomred) se encuentra vacío; El número (numdoc) se encuentra vacío'

            WHEN (c.nombre_comercial IS NULL OR LTRIM(RTRIM(c.nombre_comercial)) = '')
                 AND (c.numero_documento LIKE '%[^0-9]%')
                THEN 'El nombre del comercio (nomcomred) se encuentra vacío; El número (numdoc) contiene letras'

            WHEN (c.nombre_comercial IS NULL OR LTRIM(RTRIM(c.nombre_comercial)) = '')
                THEN 'El nombre del comercio (nomcomred) se encuentra vacío'

            WHEN (c.numero_documento IS NULL OR LTRIM(RTRIM(c.numero_documento)) = '')
                THEN 'El número de documento (numdoc) se encuentra vacío'

            WHEN (c.numero_documento LIKE '%[^0-9]%')
                THEN 'El número (numdoc) contiene letras'

            ELSE 'Registro no válido'
        END AS motivo
    FROM dbo.commerce c
    WHERE c.fecha_proceso = LTRIM(RTRIM(@fecha_proceso))
      AND (
          c.nombre_comercial IS NULL 
          OR LTRIM(RTRIM(c.nombre_comercial)) = ''
          OR c.numero_documento IS NULL 
          OR LTRIM(RTRIM(c.numero_documento)) = ''
          OR c.numero_documento LIKE '%[^0-9]%'
      );

    DECLARE @cantidad_cuarentena INT = 0;

    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO dbo.commerce_quarantine (
            id_comercio_origen,
            fecha_proceso,
            codigo_comercio,
            nombre_comercial,
            numero_documento,
            tipo_documento,
            estado,
            fecha_cuarentena,
            motivo
        )
        SELECT 
            id,
            fecha_proceso,
            codigo_comercio,
            nombre_comercial,
            numero_documento,
            tipo_documento,
            estado,
            GETDATE(),
            motivo
        FROM #registros_con_error;

        SET @cantidad_cuarentena = @@ROWCOUNT;

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

    SELECT @cantidad_cuarentena AS registros_en_cuarentena;
END;
GO

-- Procedimiento para listar comercios en cuarentena
CREATE PROCEDURE dbo.sp_obtener_comercios_cuarentena
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        id,
        id_comercio_origen,
        fecha_proceso,
        codigo_comercio,
        nombre_comercial,
        numero_documento,
        tipo_documento,
        estado,
        motivo,
        fecha_cuarentena
    FROM dbo.commerce_quarantine
    ORDER BY fecha_cuarentena DESC, id DESC;
END;
GO

-- Procedimiento para listar comercios vigentes
CREATE PROCEDURE dbo.sp_obtener_comercios
    @fecha_proceso VARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        id,
        fecha_proceso,
        codigo_comercio,
        nombre_comercial,
        numero_documento,
        tipo_documento,
        estado,
        fecha_registro
    FROM dbo.commerce
    WHERE (@fecha_proceso IS NULL OR fecha_proceso = LTRIM(RTRIM(@fecha_proceso)))
    ORDER BY fecha_registro DESC, id DESC;
END;
GO
