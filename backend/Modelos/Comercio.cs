namespace IRouteComercioApi.Modelos;

public class Comercio
{
    public int Id { get; set; }
    public string PcProcessdate { get; set; } = string.Empty;
    public string? PcCodcom { get; set; }
    public string? PcNomcomred { get; set; }
    public string? PcNumdoc { get; set; }
    public string? PcTipdoc { get; set; }
    public string? PcEstado { get; set; }
    public DateTime FechaRegistro { get; set; }
}
