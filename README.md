## 1. Arquitectura del Proyecto

```
proyectoIRoute/
├── database/                    # Scripts de Base de Datos SQL Server
│   └── estructura_base_datos.sql# Tablas y Stored Procedures
├── backend/                     # API REST en .NET (C#)
│   ├── Controladores/          # ComerciosControlador.cs
│   ├── Datos/                  # IComercioRepositorio.cs y ComercioRepositorio.cs
│   ├── Servicios/              # IComercioServicio.cs y ComercioServicio.cs
│   ├── Modelos/                # Comercio, ComercioCuarentena, RespuestaApi, etc.
│   ├── Program.cs              # Configuración de servicios, CORS y Swagger
│   └── appsettings.json        # Cadena de conexión
├── frontend/                    # Aplicación Web en Angular
│   ├── src/app/componentes/    # Carga CSV, Procesar Fecha, Cuarentena, Comercios Válidos, Login
│   ├── src/app/servicios/      # ComercioServicio y AutenticacionServicio
│   ├── src/app/modelos/        # Interfaces TypeScript
│   └── src/styles.css          # Estilos Angular
└── commerce_07102026.csv        # Archivo CSV de prueba con datos válidos e inválidos
```

---

## 2. Base de Datos (SQL Server)

Script: [`database/estructura_base_datos.sql`](database/estructura_base_datos.sql)

### 2.1 Tablas
* **`commerce`**: Contiene los registros del comercio (`pc_processdate`, `pc_codcom`, `pc_nomcomred`, `pc_numdoc`, `pc_tipdoc`, `pc_estado`, `fecha_registro`).
* **`commerce_quarantine`**: Contiene los registros observados con la columna `motivo` y `fecha_cuarentena`.

### 2.2 Procedimientos Almacenados
* **`sp_create_commerce`**: Inserta registros en la tabla `commerce`.
* **`sp_procesar_comercios_por_fecha`**: Valida que `pc_nomcomred` no esté vacío y que `pc_numdoc` no esté vacío ni contenga letras o caracteres especiales. Los registros que incumplen se trasladan a `commerce_quarantine` con su motivo y se eliminan de `commerce`. Retorna la cantidad de registros en cuarentena.
* **`sp_obtener_comercios_cuarentena`**: Lista los comercios en cuarentena con sus motivos.
* **`sp_obtener_comercios`**: Lista los comercios vigentes en `commerce`.

---

## 3. Back End (.NET API)

Endpoints en [`ComerciosControlador.cs`](backend/Controladores/ComerciosControlador.cs) (`/api/comercios`):

| Método | Ruta | Descripción |
| :--- | :--- | :--- |
| **POST** | `/api/comercios/cargar-archivo` | Recibe el CSV, valida que no esté vacío (retorna `400 BadRequest`) y registra en `commerce`. |
| **POST** | `/api/comercios/procesar-fecha` | Recibe `fechaProceso`, ejecuta la depuración y retorna la cantidad de registros en cuarentena. |
| **GET** | `/api/comercios/cuarentena` | Lista los registros en cuarentena con su motivo. |
| **GET** | `/api/comercios` | Lista los registros válidos. |

---

## 4. Front End (Angular)

* **Carga y Previsualización (`/cargar-archivo`):** Permite previsualizar el archivo CSV en una tabla antes de enviarlo.
* **Procesar por Fecha (`/procesar-fecha`):** Ejecuta la validación para la fecha indicada.
* **Cuarentena (`/cuarentena`):** Muestra los registros observados, el motivo y un buscador en tiempo real.
* **Comercios Válidos (`/comercios-activos`):** Lista los registros vigentes.
* **Inicio de Sesión (`/iniciar-sesion`):** Pantalla de acceso opcional (credenciales: `admin` / `123456`).

---

## 5. Instrucciones de Ejecución

### Base de Datos
```powershell
sqlcmd -S localhost -E -i "database\estructura_base_datos.sql"
```

### Back End (.NET)
```powershell
cd backend
dotnet run
```
API: `http://localhost:5225` | Swagger: `http://localhost:5225/`

### Front End (Angular)
```powershell
cd frontend
npm start
```
Aplicación web: `http://localhost:4200/`
