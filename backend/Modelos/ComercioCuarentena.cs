namespace IRouteComercioApi.Modelos;

public class ComercioCuarentena
{
    public int Id { get; set; }
    public int? IdComercioOrigen { get; set; }
    public string FechaProceso { get; set; } = string.Empty;
    public string? CodigoComercio { get; set; }
    public string? NombreComercial { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? TipoDocumento { get; set; }
    public string? Estado { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTime FechaCuarentena { get; set; }
}
