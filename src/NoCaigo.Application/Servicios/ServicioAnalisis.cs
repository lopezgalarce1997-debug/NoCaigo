using NoCaigo.Application.Dtos;
using NoCaigo.Application.Interfaces;
using NoCaigo.Domain.Entidades;
using NoCaigo.Domain.Enums;
using NoCaigo.Domain.Reglas;

namespace NoCaigo.Application.Servicios;

/// <summary>
/// Caso de uso principal: anonimiza, evalúa y guarda. Por ahora solo usa reglas;
/// la IA se integra en el Sprint 2 sin cambiar este contrato.
/// </summary>
public sealed class ServicioAnalisis(
    IAnonimizador anonimizador,
    EvaluadorReglas evaluadorReglas,
    IRepositorioAnalisis repositorio,
    TimeProvider reloj) : IServicioAnalisis
{
    public async Task<ResultadoAnalisisDto> AnalizarAsync(SolicitudAnalisis solicitud, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        // Se anonimiza ANTES de evaluar: las descripciones de las señales citan partes
        // del texto y se guardan, así que nunca deben contener datos personales.
        var textoAnonimizado = anonimizador.Anonimizar(solicitud.Texto);

        var resultado = evaluadorReglas.Evaluar(textoAnonimizado);
        var veredicto = ClasificadorRiesgo.ObtenerVeredicto(resultado.NivelRiesgo);

        var analisis = new Analisis(
            textoAnonimizado,
            solicitud.Canal ?? throw new ArgumentException("El canal es obligatorio.", nameof(solicitud)),
            resultado.NivelRiesgo,
            veredicto,
            resultado.TipoEstafaId,
            GenerarExplicacion(veredicto, resultado.Activadas.Count),
            usoIA: false,
            reloj.GetUtcNow().UtcDateTime);

        foreach (var regla in resultado.Activadas)
            analisis.AgregarSenal(regla.Descripcion, OrigenSenal.Regla);

        await repositorio.AgregarAsync(analisis, ct);

        return ResultadoAnalisisDto.DesdeEntidad(analisis);
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

    private static string GenerarExplicacion(Veredicto veredicto, int cantidadSenales) => veredicto switch
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
