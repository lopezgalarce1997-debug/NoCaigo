using Microsoft.Extensions.Options;
using NoCaigo.Application.Interfaces;
using NoCaigo.Domain.Entidades;
using NoCaigo.Domain.Enums;
using NoCaigo.Domain.Reglas;

namespace NoCaigo.Application.Servicios;

public sealed record ResultadoCombinado(int NivelRiesgo, Veredicto Veredicto, int TipoEstafaId, bool UsoIA);

/// <summary>
/// Combina el resultado de las reglas con el de la IA:
/// <c>final = max(reglas, PesoReglas·reglas + PesoIA·IA)</c>.
/// La IA aporta matices y puede SUBIR el riesgo, pero nunca bajarlo por debajo de lo que
/// encontraron las reglas: son deterministas y no se pueden manipular con prompt injection.
/// </summary>
public sealed class CombinadorPuntajes(IOptions<OpcionesCombinacion> opciones)
{
    /// <param name="ia">Respuesta de la IA, o null si falló o no estuvo disponible.</param>
    public ResultadoCombinado Combinar(ResultadoReglas reglas, RespuestaIA? ia)
    {
        var nivelRiesgo = ia is null ? reglas.NivelRiesgo : Ponderar(reglas.NivelRiesgo, ia.NivelRiesgo);

        // El veredicto siempre sale del puntaje FINAL (no del que propuso la IA),
        // así el veredicto y el nivel de riesgo nunca se contradicen.
        var veredicto = ClasificadorRiesgo.ObtenerVeredicto(nivelRiesgo);

        return new ResultadoCombinado(nivelRiesgo, veredicto, ElegirTipo(reglas, ia, veredicto), UsoIA: ia is not null);
    }

    private int Ponderar(int riesgoReglas, int riesgoIA)
    {
        var o = opciones.Value;
        var ponderado = (int)Math.Round(o.PesoReglas * riesgoReglas + o.PesoIA * riesgoIA, MidpointRounding.AwayFromZero);

        return Math.Clamp(Math.Max(riesgoReglas, ponderado), Analisis.NivelRiesgoMinimo, Analisis.NivelRiesgoMaximo);
    }

    // Prioridad del tipo de estafa:
    // 1. Si el mensaje es Seguro → "Ninguno" (un tipo de estafa en un mensaje seguro confunde).
    // 2. Tipo concreto de las reglas: se basa en evidencia verificable (por ejemplo, un dominio falso).
    // 3. Tipo concreto de la IA: entiende contextos que las reglas no ven (por ejemplo, "falso familiar").
    // 4. Si nadie dio un tipo concreto → "Otro".
    private static int ElegirTipo(ResultadoReglas reglas, RespuestaIA? ia, Veredicto veredicto)
    {
        if (veredicto == Veredicto.Seguro)
            return TipoEstafa.Ninguno;

        if (EsConcreto(reglas.TipoEstafaId))
            return reglas.TipoEstafaId;

        if (ia is not null && EsConcreto(ia.TipoEstafaId))
            return ia.TipoEstafaId;

        return TipoEstafa.Otro;
    }

    private static bool EsConcreto(int tipoEstafaId)
        => tipoEstafaId is not TipoEstafa.Otro and not TipoEstafa.Ninguno;
}
