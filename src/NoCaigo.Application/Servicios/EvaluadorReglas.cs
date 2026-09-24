using NoCaigo.Application.Interfaces;
using NoCaigo.Domain.Entidades;

namespace NoCaigo.Application.Servicios;

/// <summary>Resultado de aplicar todas las reglas a un texto.</summary>
public sealed record ResultadoReglas(int NivelRiesgo, int TipoEstafaId, IReadOnlyList<ResultadoRegla> Activadas);

/// <summary>
/// Ejecuta todas las reglas registradas y combina sus resultados.
/// No conoce las reglas concretas: recibe IEnumerable&lt;IReglaDeteccion&gt; por DI,
/// así agregar una regla nueva es crear la clase y registrarla (principio abierto/cerrado).
/// </summary>
public sealed class EvaluadorReglas(IEnumerable<IReglaDeteccion> reglas)
{
    public ResultadoReglas Evaluar(string texto)
    {
        var activadas = reglas
            .Select(r => r.Evaluar(texto))
            .Where(r => r.Activada)
            .ToList();

        // Los puntos se suman y se limitan a 100: varias señales juntas aumentan el riesgo.
        var nivelRiesgo = Math.Min(activadas.Sum(r => r.Puntos), Analisis.NivelRiesgoMaximo);

        return new ResultadoReglas(nivelRiesgo, ElegirTipo(activadas), activadas);
    }

    // Se usa el tipo sugerido por la regla de más puntos. Si hay señales pero ninguna
    // sugiere un tipo, es "Otro"; si no hay señales, es "Ninguno".
    private static int ElegirTipo(IReadOnlyList<ResultadoRegla> activadas)
    {
        if (activadas.Count == 0)
            return TipoEstafa.Ninguno;

        return activadas
            .Where(r => r.TipoEstafaSugeridoId is not null)
            .OrderByDescending(r => r.Puntos)
            .Select(r => r.TipoEstafaSugeridoId)
            .FirstOrDefault() ?? TipoEstafa.Otro;
    }
}
