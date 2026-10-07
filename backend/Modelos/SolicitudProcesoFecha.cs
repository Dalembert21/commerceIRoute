namespace IRouteComercioApi.Modelos;

/// <summary>
/// DTO con la fecha de proceso para la validación.
/// </summary>
public class SolicitudProcesoFecha
{
    public string FechaProceso { get; set; } = string.Empty;
}

/// <summary>
/// DTO con el resultado del procesamiento por fecha.
/// </summary>
public class ResultadoProcesoFecha
{
    public string FechaProceso { get; set; } = string.Empty;
    public int RegistrosEnCuarentena { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}
