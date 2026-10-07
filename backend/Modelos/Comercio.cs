namespace IRouteComercioApi.Modelos;

public class Comercio
{
    public int Id { get; set; }
    public string FechaProceso { get; set; } = string.Empty;
    public string? CodigoComercio { get; set; }
    public string? NombreComercial { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? TipoDocumento { get; set; }
    public string? Estado { get; set; }
    public DateTime FechaRegistro { get; set; }
}
