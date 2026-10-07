using System.Data;
using Microsoft.Data.SqlClient;
using IRouteComercioApi.Modelos;

namespace IRouteComercioApi.Datos;

/// <summary>
/// Implementación de acceso a datos mediante ADO.NET y Stored Procedures en SQL Server.
/// </summary>
public class ComercioRepositorio : IComercioRepositorio
{
    private readonly string _cadenaConexion;

    public ComercioRepositorio(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("ConexionSql")
            ?? throw new InvalidOperationException("Falta configurar la cadena ConexionSql en appsettings.json.");
    }

    /// <summary>
    /// Inserta un único registro invocando sp_create_commerce.
    /// </summary>
    public async Task<int> CrearComercioAsync(Comercio comercio)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand("dbo.sp_create_commerce", conexion)
        {
            CommandType = CommandType.StoredProcedure
        };

        comando.Parameters.AddWithValue("@pc_processdate", (object?)comercio.PcProcessdate ?? DBNull.Value);
        comando.Parameters.AddWithValue("@pc_codcom", (object?)comercio.PcCodcom ?? DBNull.Value);
        comando.Parameters.AddWithValue("@pc_nomcomred", (object?)comercio.PcNomcomred ?? DBNull.Value);
        comando.Parameters.AddWithValue("@pc_numdoc", (object?)comercio.PcNumdoc ?? DBNull.Value);
        comando.Parameters.AddWithValue("@pc_tipdoc", (object?)comercio.PcTipdoc ?? DBNull.Value);
        comando.Parameters.AddWithValue("@pc_estado", (object?)comercio.PcEstado ?? DBNull.Value);

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

                comando.Parameters.AddWithValue("@pc_processdate", (object?)comercio.PcProcessdate ?? DBNull.Value);
                comando.Parameters.AddWithValue("@pc_codcom", (object?)comercio.PcCodcom ?? DBNull.Value);
                comando.Parameters.AddWithValue("@pc_nomcomred", (object?)comercio.PcNomcomred ?? DBNull.Value);
                comando.Parameters.AddWithValue("@pc_numdoc", (object?)comercio.PcNumdoc ?? DBNull.Value);
                comando.Parameters.AddWithValue("@pc_tipdoc", (object?)comercio.PcTipdoc ?? DBNull.Value);
                comando.Parameters.AddWithValue("@pc_estado", (object?)comercio.PcEstado ?? DBNull.Value);

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

    /// <summary>
    /// Invoca el procedimiento almacenado que valida registros y traslada observados a cuarentena.
    /// </summary>
    public async Task<int> ProcesarComerciosPorFechaAsync(string fechaProceso)
    {
        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand("dbo.sp_procesar_comercios_por_fecha", conexion)
        {
            CommandType = CommandType.StoredProcedure
        };

        comando.Parameters.AddWithValue("@pc_processdate", fechaProceso);

        await conexion.OpenAsync();
        var resultado = await comando.ExecuteScalarAsync();
        return Convert.ToInt32(resultado ?? 0);
    }

    /// <summary>
    /// Consulta los comercios observados en la tabla commerce_quarantine.
    /// </summary>
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
                PcProcessdate = lector.GetString(lector.GetOrdinal("pc_processdate")),
                PcCodcom = lector.IsDBNull(lector.GetOrdinal("pc_codcom")) ? null : lector.GetString(lector.GetOrdinal("pc_codcom")),
                PcNomcomred = lector.IsDBNull(lector.GetOrdinal("pc_nomcomred")) ? null : lector.GetString(lector.GetOrdinal("pc_nomcomred")),
                PcNumdoc = lector.IsDBNull(lector.GetOrdinal("pc_numdoc")) ? null : lector.GetString(lector.GetOrdinal("pc_numdoc")),
                PcTipdoc = lector.IsDBNull(lector.GetOrdinal("pc_tipdoc")) ? null : lector.GetString(lector.GetOrdinal("pc_tipdoc")),
                PcEstado = lector.IsDBNull(lector.GetOrdinal("pc_estado")) ? null : lector.GetString(lector.GetOrdinal("pc_estado")),
                Motivo = lector.GetString(lector.GetOrdinal("motivo")),
                FechaCuarentena = lector.GetDateTime(lector.GetOrdinal("fecha_cuarentena"))
            });
        }

        return lista;
    }

    /// <summary>
    /// Consulta los comercios vigentes en la tabla commerce.
    /// </summary>
    public async Task<IEnumerable<Comercio>> ObtenerComerciosAsync(string? fechaProceso = null)
    {
        var lista = new List<Comercio>();

        await using var conexion = new SqlConnection(_cadenaConexion);
        await using var comando = new SqlCommand("dbo.sp_obtener_comercios", conexion)
        {
            CommandType = CommandType.StoredProcedure
        };

        comando.Parameters.AddWithValue("@pc_processdate", (object?)fechaProceso ?? DBNull.Value);

        await conexion.OpenAsync();
        await using var lector = await comando.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new Comercio
            {
                Id = lector.GetInt32(lector.GetOrdinal("id")),
                PcProcessdate = lector.GetString(lector.GetOrdinal("pc_processdate")),
                PcCodcom = lector.IsDBNull(lector.GetOrdinal("pc_codcom")) ? null : lector.GetString(lector.GetOrdinal("pc_codcom")),
                PcNomcomred = lector.IsDBNull(lector.GetOrdinal("pc_nomcomred")) ? null : lector.GetString(lector.GetOrdinal("pc_nomcomred")),
                PcNumdoc = lector.IsDBNull(lector.GetOrdinal("pc_numdoc")) ? null : lector.GetString(lector.GetOrdinal("pc_numdoc")),
                PcTipdoc = lector.IsDBNull(lector.GetOrdinal("pc_tipdoc")) ? null : lector.GetString(lector.GetOrdinal("pc_tipdoc")),
                PcEstado = lector.IsDBNull(lector.GetOrdinal("pc_estado")) ? null : lector.GetString(lector.GetOrdinal("pc_estado")),
                FechaRegistro = lector.GetDateTime(lector.GetOrdinal("fecha_registro"))
            });
        }

        return lista;
    }
}
