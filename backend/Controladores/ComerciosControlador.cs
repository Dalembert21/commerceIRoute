using Microsoft.AspNetCore.Mvc;
using IRouteComercioApi.Modelos;
using IRouteComercioApi.Servicios;

namespace IRouteComercioApi.Controladores;

[ApiController]
[Route("api/comercios")]
[Produces("application/json")]
public class ComerciosControlador : ControllerBase
{
    private readonly IComercioServicio _servicio;

    public ComerciosControlador(IComercioServicio servicio)
    {
        _servicio = servicio;
    }

    // Recibe el archivo CSV, valida que contenga datos y registra en la tabla commerce
    [HttpPost("cargar-archivo")]
    public async Task<IActionResult> CargarArchivo([FromForm] IFormFile? archivo)
    {
        if (archivo == null || archivo.Length == 0)
        {
            return BadRequest(RespuestaApi<string>.Fallo("El archivo recibido se encuentra vacío o no fue enviado."));
        }

        try
        {
            var (totalRegistros, mensaje) = await _servicio.ProcesarCargaArchivoAsync(archivo);
            return Ok(RespuestaApi<int>.Correcto(totalRegistros, mensaje, totalRegistros));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(RespuestaApi<string>.Fallo(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<string>.Fallo($"Error al registrar comercios: {ex.Message}"));
        }
    }

    // Recibe la fecha y ejecuta la validación para mover registros a cuarentena
    [HttpPost("procesar-fecha")]
    public async Task<IActionResult> ProcesarPorFecha([FromBody] SolicitudProcesoFecha solicitud)
    {
        if (solicitud == null || string.IsNullOrWhiteSpace(solicitud.FechaProceso))
        {
            return BadRequest(RespuestaApi<string>.Fallo("Debe ingresar la fecha de proceso."));
        }

        try
        {
            var resultado = await _servicio.ProcesarComerciosPorFechaAsync(solicitud.FechaProceso);
            return Ok(RespuestaApi<ResultadoProcesoFecha>.Correcto(
                resultado,
                resultado.Mensaje,
                resultado.RegistrosEnCuarentena
            ));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(RespuestaApi<string>.Fallo(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<string>.Fallo($"Error al procesar comercios: {ex.Message}"));
        }
    }

    // Obtiene los comercios observados en la tabla commerce_quarantine
    [HttpGet("cuarentena")]
    public async Task<IActionResult> ListarCuarentena()
    {
        try
        {
            var lista = await _servicio.ListarComerciosCuarentenaAsync();
            var total = lista.Count();
            return Ok(RespuestaApi<IEnumerable<ComercioCuarentena>>.Correcto(
                lista,
                $"Se encontraron {total} registros en cuarentena.",
                total
            ));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<string>.Fallo($"Error al consultar registros de cuarentena: {ex.Message}"));
        }
    }

    // Obtiene los comercios vigentes en la tabla commerce
    [HttpGet]
    public async Task<IActionResult> ListarComercios([FromQuery] string? fechaProceso = null)
    {
        try
        {
            var lista = await _servicio.ListarComerciosAsync(fechaProceso);
            var total = lista.Count();
            return Ok(RespuestaApi<IEnumerable<Comercio>>.Correcto(
                lista,
                $"Se encontraron {total} comercios registrados.",
                total
            ));
        }
        catch (Exception ex)
        {
            return StatusCode(500, RespuestaApi<string>.Fallo($"Error al consultar comercios: {ex.Message}"));
        }
    }
}
