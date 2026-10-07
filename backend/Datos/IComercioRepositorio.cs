using IRouteComercioApi.Modelos;

namespace IRouteComercioApi.Datos;

/// <summary>
/// Contrato para el acceso a datos de comercios y su cuarentena.
/// </summary>
public interface IComercioRepositorio
{
    Task<int> CrearComercioAsync(Comercio comercio);
    Task<int> CrearComerciosLoteAsync(IEnumerable<Comercio> comercios);
    Task<int> ProcesarComerciosPorFechaAsync(string fechaProceso);
    Task<IEnumerable<ComercioCuarentena>> ObtenerComerciosCuarentenaAsync();
    Task<IEnumerable<Comercio>> ObtenerComerciosAsync(string? fechaProceso = null);
}
