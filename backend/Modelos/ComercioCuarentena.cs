namespace IRouteComercioApi.Modelos;

/// <summary>
/// Modelo representativo de la tabla 'commerce_quarantine', con columna motivo de rechazo.
/// </summary>
public class ComercioCuarentena
{
    public int Id { get; set; }
    public int? IdComercioOrigen { get; set; }
    public string PcProcessdate { get; set; } = string.Empty;
    public string? PcCodcom { get; set; }
    public string? PcNomcomred { get; set; }
    public string? PcNumdoc { get; set; }
    public string? PcTipdoc { get; set; }
    public string? PcEstado { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTime FechaCuarentena { get; set; }
}
