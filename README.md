# Solución Técnica: Módulo de Gestión y Depuración de Comercios (iRoute)

Aplicación Web Full Stack desarrollada con **.NET (API REST)**, **Angular 19 (Frontend)** y **SQL Server (Base de Datos)** para la carga, previsualización, validación y depuración transaccional de comercios a partir de archivos CSV.

---

## 1. Arquitectura de la Solución

El proyecto sigue una arquitectura limpia de responsabilidades desacopladas:

```
proyectoIRoute/
├── database/
│   └── estructura_base_datos.sql      # Script DDL/DML: Creación de BD, tablas y SPs oficiales
├── backend/                           # API REST en .NET
│   ├── Controladores/                 # ComerciosControlador.cs (Endpoints REST y códigos HTTP)
│   ├── Datos/                         # IComercioRepositorio.cs y ComercioRepositorio.cs (ADO.NET)
│   ├── Servicios/                     # IComercioServicio.cs y ComercioServicio.cs (Reglas y validaciones)
│   ├── Modelos/                       # Comercio, ComercioCuarentena, RespuestaApi, etc.
│   ├── IRouteComercioApi.http         # Peticiones HTTP documentadas para pruebas de integración
│   ├── Program.cs                     # Inyección de dependencias, CORS y Swagger
│   └── appsettings.json               # Cadena de conexión
├── frontend/                          # Single Page Application (SPA) en Angular 19
│   ├── src/environments/              # Configuración de URLs de API desacoplada
│   ├── src/app/componentes/           # Componentes Standalone (Carga, Proceso, Cuarentena, Válidos, Login)
│   ├── src/app/servicios/             # ComercioServicio y AutenticacionServicio
│   ├── src/app/modelos/               # Interfaces y DTOs alineados al contrato pc_*
│   └── src/styles.css                 # Sistema de diseño y variables CSS
└── commerce_07102026.csv              # Archivo CSV de prueba oficial (casos válidos e inválidos)
```

---

## 2. Base de Datos (SQL Server)

Script de ejecución: [`database/estructura_base_datos.sql`](database/estructura_base_datos.sql)

### 2.1 Modelo de Datos (Data Contract Oficial)
Para cumplir con los requerimientos del enunciado técnico (*"la estructura de la tabla es la misma del csv"*), las tablas respetan los nombres de columnas oficiales:

* **`dbo.commerce`**:
  * `id` (INT, IDENTITY, PK)
  * `pc_processdate` (VARCHAR(20), NOT NULL): Fecha de proceso.
  * `pc_codcom` (VARCHAR(20), NULL): Código del comercio.
  * `pc_nomcomred` (VARCHAR(150), NULL): Nombre comercial.
  * `pc_numdoc` (VARCHAR(20), NULL): Número de documento.
  * `pc_tipdoc` (VARCHAR(10), NULL): Tipo de documento (RUC, DNI).
  * `pc_estado` (VARCHAR(20), NULL): Estado del comercio.
  * `fecha_registro` (DATETIME, NOT NULL): Fecha de auditoría.

* **`dbo.commerce_quarantine`**:
  * Incluye la estructura origen más:
    * `motivo` (VARCHAR(250), NOT NULL): Motivo detallado del rechazo.
    * `fecha_cuarentena` (DATETIME, NOT NULL): Fecha del traslado a cuarentena.

### 2.2 Procedimientos Almacenados
* **`dbo.sp_create_commerce`**: Registra un comercio en la tabla `commerce`.
* **`dbo.sp_procesar_comercios_por_fecha`**:
  * Recibe `@pc_processdate`.
  * Valida que `pc_nomcomred` no esté vacío.
  * Valida que `pc_numdoc` no esté vacío, ni contenga letras ni caracteres especiales (`LIKE '%[^0-9]%'`).
  * En una sola **transacción atómica (`BEGIN TRANSACTION / COMMIT / ROLLBACK`)**, inserta en `commerce_quarantine` con el motivo exacto y elimina los registros observados de `commerce`.
  * Retorna la cantidad de registros enviados a cuarentena.
* **`dbo.sp_obtener_comercios_cuarentena`**: Lista los registros en cuarentena ordenados cronológicamente.
* **`dbo.sp_obtener_comercios`**: Lista los comercios vigentes.

---

## 3. Back End (.NET API REST)

Endpoints expuestos bajo `/api/comercios`:

| Método | Ruta | Descripción | HTTP Status |
| :--- | :--- | :--- | :---: |
| **POST** | `/api/comercios/cargar-archivo` | Recibe el CSV `commerce_DDMMYYYY.csv`. Valida nombre, tamaño, extensión y contenido no vacío. Inserta registros mediante `sp_create_commerce`. | `200 OK` / `400 BadRequest` |
| **POST** | `/api/comercios/procesar-fecha` | Recibe `{ "fechaProceso": "DD/MM/YYYY" }`, ejecuta la validación en el SP y devuelve la cantidad en cuarentena. | `200 OK` / `400 BadRequest` |
| **GET** | `/api/comercios/cuarentena` | Lista los comercios rechazados con sus motivos. | `200 OK` |
| **GET** | `/api/comercios` | Lista los comercios vigentes en la tabla `commerce`. | `200 OK` |

### Decisión de Arquitectura: Invocación de `sp_create_commerce`
El enunciado solicitó registrar los datos *"mediante la invocación de un store procedure sp_create_commerce"*.  
Para respetar esta especificación asegurando integridad, se implementó en [ComercioRepositorio.cs](backend/Datos/ComercioRepositorio.cs) una invocación iterativa envuelta en una **`SqlTransaction` única**.  
*En un entorno productivo de alto volumen (+100k registros), la optimización recomendada sería utilizar un **Table-Valued Parameter (TVP)** o **`SqlBulkCopy`**.*

---

## 4. Front End (Angular 19)

* **Carga y Previsualización (`/cargar-archivo`):**
  * Validación previa de nombre de archivo (`commerce_DDMMYYYY.csv`), tamaño (< 5MB) y contenido.
  * Lectura en memoria con `FileReader` para previsualizar los registros en tabla interactiva antes del envío HTTP.
* **Procesar por Fecha (`/procesar-fecha`):**
  * Detección automática y sugerencia de fechas disponibles en la base de datos mediante botones interactivos (chips).
  * Ejecución de validación y retroalimentación con el conteo de registros derivados a cuarentena.
* **Registros en Cuarentena (`/cuarentena`):**
  * Tabla con motivos detallados de error, tarjetas con métricas en tiempo real y buscador en tiempo real.
* **Comercios Válidos (`/comercios-activos`):**
  * Visualización de registros vigentes en la tabla principal.
* **Inicio de Sesión Opcional (`/iniciar-sesion`):**
  * **Usuario:** `admin` | **Contraseña:** `123456`
  * (Opcional también `evaluador` / `123456`). Incluye botones de acceso rápido de 1 clic en la interfaz.

---

## 5. Instrucciones de Despliegue y Ejecución

### 1. Base de Datos
Ejecutar el script SQL en una instancia local de SQL Server:
```powershell
sqlcmd -S localhost -E -i "database\estructura_base_datos.sql"
```

### 2. Back End (.NET)
```powershell
cd backend
dotnet run
```
* **API Base:** `http://localhost:5225`
* **Swagger UI:** `http://localhost:5225/`

### 3. Front End (Angular)
```powershell
cd frontend
npm start
```
* **Aplicación Web:** `http://localhost:4200/`
