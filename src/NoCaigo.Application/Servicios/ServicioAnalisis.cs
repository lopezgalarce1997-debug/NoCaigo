using Microsoft.Extensions.Logging;
using NoCaigo.Application.Dtos;
using NoCaigo.Application.Interfaces;
using NoCaigo.Domain.Entidades;
using NoCaigo.Domain.Enums;

namespace NoCaigo.Application.Servicios;

/// <summary>
/// Caso de uso principal: anonimiza, evalúa con reglas e IA, combina y guarda.
/// Si la IA falla, el análisis sigue igual solo con reglas (degradación elegante).
/// </summary>
public sealed class ServicioAnalisis(
    IAnonimizador anonimizador,
    EvaluadorReglas evaluadorReglas,
    IServicioIA servicioIA,
    CombinadorPuntajes combinador,
    IRepositorioAnalisis repositorio,
    TimeProvider reloj,
    ILogger<ServicioAnalisis> logger) : IServicioAnalisis
{
    public const string AvisoSinIA =
        "El análisis con inteligencia artificial no estuvo disponible, así que este resultado se basa solo en reglas automáticas.";

    public async Task<ResultadoAnalisisDto> AnalizarAsync(SolicitudAnalisis solicitud, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(solicitud);
        var canal = solicitud.Canal ?? throw new ArgumentException("El canal es obligatorio.", nameof(solicitud));

        // Se anonimiza ANTES de todo: ni las reglas (que citan el texto en sus señales)
        // ni la IA (un servicio externo) ven nunca los datos personales.
        var textoAnonimizado = anonimizador.Anonimizar(solicitud.Texto);

        var reglas = evaluadorReglas.Evaluar(textoAnonimizado);
        var ia = await ConsultarIAAsync(textoAnonimizado, ct);
        var resultado = combinador.Combinar(reglas, ia);

        var analisis = new Analisis(
            textoAnonimizado,
            canal,
            resultado.NivelRiesgo,
            resultado.Veredicto,
            resultado.TipoEstafaId,
            GenerarExplicacion(resultado, ia, reglas.Activadas.Count),
            resultado.UsoIA,
            reloj.GetUtcNow().UtcDateTime);

        foreach (var regla in reglas.Activadas)
            analisis.AgregarSenal(regla.Descripcion, OrigenSenal.Regla);

        // Si el veredicto final es Seguro, las "señales" de la IA suelen ser observaciones
        // ("no hay indicios de estafa") que confunden más de lo que ayudan: se omiten.
        if (resultado.Veredicto != Veredicto.Seguro)
        {
            foreach (var senal in ia?.Senales ?? [])
                analisis.AgregarSenal(senal, OrigenSenal.IA);
        }

        await repositorio.AgregarAsync(analisis, ct);

        return ResultadoAnalisisDto.DesdeEntidad(analisis);
    }

    /// <summary>Devuelve la respuesta de la IA, o null si falló (error, timeout, cuota o JSON inválido).</summary>
    private async Task<RespuestaIA?> ConsultarIAAsync(string textoAnonimizado, CancellationToken ct)
    {
        try
        {
            return await servicioIA.AnalizarAsync(textoAnonimizado, ct);
        }
        catch (ExcepcionServicioIA ex)
        {
            // Solo se captura la falla esperada de la IA. Un error de programación o una
            // cancelación del cliente siguen su curso y no se esconden como "sin IA".
            logger.LogWarning(ex, "Análisis sin IA: {Motivo}", ex.Message);
            return null;
        }
    }

    public async Task<ResultadoAnalisisDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        var analisis = await repositorio.ObtenerPorIdAsync(id, ct);
        return analisis is null ? null : ResultadoAnalisisDto.DesdeEntidad(analisis);
    }

    public async Task<ResultadoPaginado<AnalisisResumenDto>> ListarAsync(FiltroAnalisis filtro, CancellationToken ct = default)
    {
        var (items, total) = await repositorio.ListarAsync(filtro, ct);

        return new ResultadoPaginado<AnalisisResumenDto>(
            items.Select(AnalisisResumenDto.DesdeEntidad).ToList(),
            filtro.Pagina,
            filtro.TamanoPagina,
            total);
    }

    private static string GenerarExplicacion(ResultadoCombinado resultado, RespuestaIA? ia, int senalesReglas)
    {
        // Con IA se usa su explicación (más natural), salvo que venga vacía o contradiga
        // el veredicto final (por ejemplo, la IA fue manipulada y dijo "es seguro").
        if (ia is not null && !string.IsNullOrWhiteSpace(ia.Explicacion) && ia.Veredicto == resultado.Veredicto)
            return ia.Explicacion;

        var explicacion = ExplicacionPorVeredicto(resultado.Veredicto, senalesReglas + (ia?.Senales.Count ?? 0));
        return resultado.UsoIA ? explicacion : $"{explicacion} {AvisoSinIA}";
    }

    private static string ExplicacionPorVeredicto(Veredicto veredicto, int cantidadSenales) => veredicto switch
    {
        Veredicto.Estafa =>
            $"Encontramos {cantidadSenales} señales típicas de estafa. No hagas clic en links, no entregues datos " +
            "y no transfieras dinero. Si dice venir de una empresa, contáctala por sus canales oficiales.",
        Veredicto.Sospechoso =>
            $"Encontramos {cantidadSenales} señal(es) de riesgo. Antes de responder, verifica directamente " +
            "con la entidad por un canal oficial.",
        _ when cantidadSenales > 0 =>
            "Encontramos señales leves, pero no suficientes para considerarlo sospechoso. Mantén la precaución.",
        _ =>
            "No encontramos señales de estafa conocidas. Aun así, si tienes dudas, verifica con la entidad oficial."
    };
}
