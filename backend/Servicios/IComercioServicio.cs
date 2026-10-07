using IRouteComercioApi.Modelos;

namespace IRouteComercioApi.Servicios;

/// <summary>
/// Contrato de los servicios del módulo de comercios.
/// </summary>
public interface IComercioServicio
{
    Task<(int TotalCargados, string Mensaje)> ProcesarCargaArchivoAsync(IFormFile archivo);
    Task<ResultadoProcesoFecha> ProcesarComerciosPorFechaAsync(string fechaProceso);
    Task<IEnumerable<ComercioCuarentena>> ListarComerciosCuarentenaAsync();
    Task<IEnumerable<Comercio>> ListarComerciosAsync(string? fechaProceso = null);
}
