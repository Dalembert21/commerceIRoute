using System.Text;
using System.Text.RegularExpressions;
using IRouteComercioApi.Datos;
using IRouteComercioApi.Modelos;

namespace IRouteComercioApi.Servicios;

public class ComercioServicio : IComercioServicio
{
    private readonly IComercioRepositorio _repositorio;
    private const long TamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB

    // Patrón solicitado por el enunciado: commerce_DDMMYYYY.csv (ej: commerce_07102026.csv)
    private static readonly Regex PatronNombreArchivo = new(@"^commerce_\d{8}\.csv$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public ComercioServicio(IComercioRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<(int TotalCargados, string Mensaje)> ProcesarCargaArchivoAsync(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
        {
            throw new ArgumentException("El archivo se encuentra vacío o no fue enviado.");
        }

        if (archivo.Length > TamanoMaximoBytes)
        {
            throw new ArgumentException("El archivo excede el tamaño máximo permitido de 5 MB.");
        }

        // Validación estricta del nombre solicitado en el enunciado: commerce_DDMMYYYY.csv
        if (!PatronNombreArchivo.IsMatch(archivo.FileName))
        {
            throw new ArgumentException("El nombre del archivo debe cumplir con el formato requerido: commerce_DDMMYYYY.csv (por ejemplo: commerce_07102026.csv).");
        }

        var comercios = new List<Comercio>();

        using (var lector = new StreamReader(archivo.OpenReadStream(), Encoding.UTF8))
        {
            var primeraLinea = await lector.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(primeraLinea))
            {
                throw new ArgumentException("El archivo CSV no contiene encabezados o está vacío.");
            }

            char separador = primeraLinea.Contains(';') ? ';' : ',';
            var cabeceras = primeraLinea.Split(separador)
                .Select(c => c.Trim().ToLowerInvariant().Trim('\"', '\''))
                .ToList();

            // Mapeo tolerante priorizando nombres oficiales del contrato pc_*
            int idxFecha = cabeceras.FindIndex(c => c.Contains("processdate") || c.Contains("fecha"));
            int idxCodigo = cabeceras.FindIndex(c => c.Contains("codcom") || c.Contains("codigo"));
            int idxNombre = cabeceras.FindIndex(c => c.Contains("nomcomred") || c.Contains("nombre"));
            int idxDocumento = cabeceras.FindIndex(c => c.Contains("numdoc") || c.Contains("documento"));
            int idxTipo = cabeceras.FindIndex(c => c.Contains("tipdoc") || c.Contains("tipo"));
            int idxEstado = cabeceras.FindIndex(c => c.Contains("estado"));

            // Posiciones por defecto en caso de encabezados sin coincidencias directas
            if (idxFecha == -1) idxFecha = 0;
            if (idxCodigo == -1 && cabeceras.Count > 1) idxCodigo = 1;
            if (idxNombre == -1 && cabeceras.Count > 2) idxNombre = 2;
            if (idxDocumento == -1 && cabeceras.Count > 3) idxDocumento = 3;
            if (idxTipo == -1 && cabeceras.Count > 4) idxTipo = 4;
            if (idxEstado == -1 && cabeceras.Count > 5) idxEstado = 5;

            string? linea;
            while ((linea = await lector.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;

                var campos = linea.Split(separador)
                    .Select(c => c.Trim().Trim('\"', '\''))
                    .ToArray();

                string fecha = ObtenerCampo(campos, idxFecha, 20);
                string? codigo = ObtenerCampoOpcional(campos, idxCodigo, 20);
                string? nombre = ObtenerCampoOpcional(campos, idxNombre, 150);
                string? documento = ObtenerCampoOpcional(campos, idxDocumento, 20);
                string? tipo = ObtenerCampoOpcional(campos, idxTipo, 10);
                string? estado = ObtenerCampoOpcional(campos, idxEstado, 20);

                if (string.IsNullOrWhiteSpace(fecha))
                {
                    fecha = DateTime.Now.ToString("dd/MM/yyyy");
                }

                comercios.Add(new Comercio
                {
                    PcProcessdate = fecha,
                    PcCodcom = codigo,
                    PcNomcomred = nombre,
                    PcNumdoc = documento,
                    PcTipdoc = tipo,
                    PcEstado = estado,
                    FechaRegistro = DateTime.Now
                });
            }
        }

        if (comercios.Count == 0)
        {
            throw new ArgumentException("El archivo CSV no contiene registros para procesar.");
        }

        int totalInsertados = await _repositorio.CrearComerciosLoteAsync(comercios);

        return (totalInsertados, $"Se cargaron exitosamente {totalInsertados} comercios en la tabla commerce.");
    }

    public async Task<ResultadoProcesoFecha> ProcesarComerciosPorFechaAsync(string fechaProceso)
    {
        if (string.IsNullOrWhiteSpace(fechaProceso))
        {
            throw new ArgumentException("La fecha de proceso es requerida.");
        }

        fechaProceso = fechaProceso.Trim();
        if (fechaProceso.Length > 20)
        {
            throw new ArgumentException("El formato de fecha no es válido.");
        }

        int enCuarentena = await _repositorio.ProcesarComerciosPorFechaAsync(fechaProceso);

        string mensaje = enCuarentena > 0
            ? $"Proceso completado. Se trasladaron {enCuarentena} registros a la tabla commerce_quarantine."
            : "Proceso completado. Todos los registros cumplen las condiciones, ninguno fue a cuarentena.";

        return new ResultadoProcesoFecha
        {
            FechaProceso = fechaProceso,
            RegistrosEnCuarentena = enCuarentena,
            Mensaje = mensaje
        };
    }

    public async Task<IEnumerable<ComercioCuarentena>> ListarComerciosCuarentenaAsync()
    {
        return await _repositorio.ObtenerComerciosCuarentenaAsync();
    }

    public async Task<IEnumerable<Comercio>> ListarComerciosAsync(string? fechaProceso = null)
    {
        return await _repositorio.ObtenerComerciosAsync(fechaProceso?.Trim());
    }

    private static string ObtenerCampo(string[] campos, int indice, int longitudMaxima)
    {
        if (indice >= 0 && indice < campos.Length && !string.IsNullOrWhiteSpace(campos[indice]))
        {
            string valor = campos[indice].Trim();
            return valor.Length > longitudMaxima ? valor.Substring(0, longitudMaxima) : valor;
        }
        return string.Empty;
    }

    private static string? ObtenerCampoOpcional(string[] campos, int indice, int longitudMaxima)
    {
        var valor = ObtenerCampo(campos, indice, longitudMaxima);
        return string.IsNullOrWhiteSpace(valor) ? null : valor;
    }
}
