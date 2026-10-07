using System.Data;
using Microsoft.Data.SqlClient;
using IRouteComercioApi.Modelos;

namespace IRouteComercioApi.Datos;

// Repositorio que ejecuta los procedimientos almacenados en SQL Server
public class ComercioRepositorio : IComercioRepositorio
{
    private readonly string _cadenaConexion;

    public ComercioRepositorio(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("ConexionSql")
            ?? throw new InvalidOperationException("Falta configurar la cadena ConexionSql en appsettings.json.");
    }

    // Inserta un único comercio invocando sp_create_commerce
    public async Task<int> CrearComercioAsync(Comercio comercio)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand("dbo.sp_create_commerce", conexion)
        {
            CommandType = CommandType.StoredProcedure
        };

        comando.Parameters.AddWithValue("@fecha_proceso", (object?)comercio.FechaProceso ?? DBNull.Value);
        comando.Parameters.AddWithValue("@codigo_comercio", (object?)comercio.CodigoComercio ?? DBNull.Value);
        comando.Parameters.AddWithValue("@nombre_comercial", (object?)comercio.NombreComercial ?? DBNull.Value);
        comando.Parameters.AddWithValue("@numero_documento", (object?)comercio.NumeroDocumento ?? DBNull.Value);
        comando.Parameters.AddWithValue("@tipo_documento", (object?)comercio.TipoDocumento ?? DBNull.Value);
        comando.Parameters.AddWithValue("@estado", (object?)comercio.Estado ?? DBNull.Value);

        await conexion.OpenAsync();
        var resultado = await comando.ExecuteScalarAsync();
        return Convert.ToInt32(resultado);
    }

    // Inserta la lista de comercios dentro de una transacción para evitar inconsistencias
    public async Task<int> CrearComerciosLoteAsync(IEnumerable<Comercio> comercios)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        await conexion.OpenAsync();
        await using var transaccion = (SqlTransaction)await conexion.BeginTransactionAsync();

        int totalInsertados = 0;
        try
        {
            foreach (var comercio in comercios)
            {
                await using var comando = new SqlCommand("dbo.sp_create_commerce", conexion, transaccion)
                {
                    CommandType = CommandType.StoredProcedure
                };

                comando.Parameters.AddWithValue("@fecha_proceso", (object?)comercio.FechaProceso ?? DBNull.Value);
                comando.Parameters.AddWithValue("@codigo_comercio", (object?)comercio.CodigoComercio ?? DBNull.Value);
                comando.Parameters.AddWithValue("@nombre_comercial", (object?)comercio.NombreComercial ?? DBNull.Value);
                comando.Parameters.AddWithValue("@numero_documento", (object?)comercio.NumeroDocumento ?? DBNull.Value);
                comando.Parameters.AddWithValue("@tipo_documento", (object?)comercio.TipoDocumento ?? DBNull.Value);
                comando.Parameters.AddWithValue("@estado", (object?)comercio.Estado ?? DBNull.Value);

                await comando.ExecuteNonQueryAsync();
                totalInsertados++;
            }

            await transaccion.CommitAsync();
            return totalInsertados;
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }

    // Ejecuta el procedimiento para validar registros por fecha y enviarlos a cuarentena
    public async Task<int> ProcesarComerciosPorFechaAsync(string fechaProceso)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand("dbo.sp_procesar_comercios_por_fecha", conexion)
        {
            CommandType = CommandType.StoredProcedure
        };

        comando.Parameters.AddWithValue("@fecha_proceso", fechaProceso);

        await conexion.OpenAsync();
        var resultado = await comando.ExecuteScalarAsync();
        return Convert.ToInt32(resultado ?? 0);
    }

    // Consulta la tabla commerce_quarantine mediante store procedure
    public async Task<IEnumerable<ComercioCuarentena>> ObtenerComerciosCuarentenaAsync()
    {
        var lista = new List<ComercioCuarentena>();

        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand("dbo.sp_obtener_comercios_cuarentena", conexion)
        {
            CommandType = CommandType.StoredProcedure
        };

        await conexion.OpenAsync();
        await using var lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new ComercioCuarentena
            {
                Id = lector.GetInt32(lector.GetOrdinal("id")),
                IdComercioOrigen = lector.IsDBNull(lector.GetOrdinal("id_comercio_origen")) ? null : lector.GetInt32(lector.GetOrdinal("id_comercio_origen")),
                FechaProceso = lector.GetString(lector.GetOrdinal("fecha_proceso")),
                CodigoComercio = lector.IsDBNull(lector.GetOrdinal("codigo_comercio")) ? null : lector.GetString(lector.GetOrdinal("codigo_comercio")),
                NombreComercial = lector.IsDBNull(lector.GetOrdinal("nombre_comercial")) ? null : lector.GetString(lector.GetOrdinal("nombre_comercial")),
                NumeroDocumento = lector.IsDBNull(lector.GetOrdinal("numero_documento")) ? null : lector.GetString(lector.GetOrdinal("numero_documento")),
                TipoDocumento = lector.IsDBNull(lector.GetOrdinal("tipo_documento")) ? null : lector.GetString(lector.GetOrdinal("tipo_documento")),
                Estado = lector.IsDBNull(lector.GetOrdinal("estado")) ? null : lector.GetString(lector.GetOrdinal("estado")),
                Motivo = lector.GetString(lector.GetOrdinal("motivo")),
                FechaCuarentena = lector.GetDateTime(lector.GetOrdinal("fecha_cuarentena"))
            });
        }

        return lista;
    }

    // Consulta los comercios válidos de la tabla commerce
    public async Task<IEnumerable<Comercio>> ObtenerComerciosAsync(string? fechaProceso = null)
    {
        var lista = new List<Comercio>();

        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand("dbo.sp_obtener_comercios", conexion)
        {
            CommandType = CommandType.StoredProcedure
        };

        comando.Parameters.AddWithValue("@fecha_proceso", (object?)fechaProceso ?? DBNull.Value);

        await conexion.OpenAsync();
        await using var lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new Comercio
            {
                Id = lector.GetInt32(lector.GetOrdinal("id")),
                FechaProceso = lector.GetString(lector.GetOrdinal("fecha_proceso")),
                CodigoComercio = lector.IsDBNull(lector.GetOrdinal("codigo_comercio")) ? null : lector.GetString(lector.GetOrdinal("codigo_comercio")),
                NombreComercial = lector.IsDBNull(lector.GetOrdinal("nombre_comercial")) ? null : lector.GetString(lector.GetOrdinal("nombre_comercial")),
                NumeroDocumento = lector.IsDBNull(lector.GetOrdinal("numero_documento")) ? null : lector.GetString(lector.GetOrdinal("numero_documento")),
                TipoDocumento = lector.IsDBNull(lector.GetOrdinal("tipo_documento")) ? null : lector.GetString(lector.GetOrdinal("tipo_documento")),
                Estado = lector.IsDBNull(lector.GetOrdinal("estado")) ? null : lector.GetString(lector.GetOrdinal("estado")),
                FechaRegistro = lector.GetDateTime(lector.GetOrdinal("fecha_registro"))
            });
        }

        return lista;
    }
}
