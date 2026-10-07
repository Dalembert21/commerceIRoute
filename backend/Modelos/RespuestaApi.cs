namespace IRouteComercioApi.Modelos;

/// <summary>
/// Estructura estándar para las respuestas de la API.
/// </summary>
public class RespuestaApi<T>
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public T? Datos { get; set; }
    public int? TotalRegistros { get; set; }

    public static RespuestaApi<T> Correcto(T datos, string mensaje = "Operación realizada con éxito.", int? total = null)
    {
        return new RespuestaApi<T>
        {
            Exito = true,
            Mensaje = mensaje,
            Datos = datos,
            TotalRegistros = total
        };
    }

    public static RespuestaApi<T> Fallo(string mensaje)
    {
        return new RespuestaApi<T>
        {
            Exito = false,
            Mensaje = mensaje,
            Datos = default,
            TotalRegistros = 0
        };
    }
}
