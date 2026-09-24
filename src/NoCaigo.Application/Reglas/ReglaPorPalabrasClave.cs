using System.Text;
using System.Text.RegularExpressions;
using NoCaigo.Application.Interfaces;

namespace NoCaigo.Application.Reglas;

/// <summary>
/// Base para las reglas que buscan palabras o frases (patrón Template Method):
/// la búsqueda es común y cada regla solo define sus patrones, puntos y mensaje.
/// </summary>
public abstract class ReglaPorPalabrasClave : IReglaDeteccion
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(200);

    private readonly Regex _regex;

    /// <param name="patrones">
    /// Expresiones regulares en minúsculas y sin tildes. Se buscan como palabras
    /// completas, así "hoy" no se activa dentro de "hoyo".
    /// </param>
    protected ReglaPorPalabrasClave(IEnumerable<string> patrones)
    {
        _regex = new Regex(
            $@"\b(?:{string.Join("|", patrones)})\b",
            RegexOptions.Compiled | RegexOptions.CultureInvariant,
            Timeout);
    }

    protected abstract int Puntos { get; }

    protected virtual int? TipoEstafaSugeridoId => null;

    /// <param name="encontradas">Frases encontradas, tal como aparecen en el texto original.</param>
    protected abstract string Describir(IReadOnlyList<string> encontradas);

    public ResultadoRegla Evaluar(string texto)
    {
        ArgumentNullException.ThrowIfNull(texto);

        var original = texto.Normalize(NormalizationForm.FormC);
        var normalizado = NormalizadorTexto.Normalizar(original);

        // Si la normalización cambió el largo (caso raro), se muestran las frases normalizadas.
        var mismoLargo = original.Length == normalizado.Length;

        var encontradas = _regex.Matches(normalizado)
            .Select(m => mismoLargo ? original.Substring(m.Index, m.Length) : m.Value)
            .DistinctBy(f => f.ToLowerInvariant())
            .ToList();

        return encontradas.Count == 0
            ? ResultadoRegla.NoActivada
            : new ResultadoRegla(true, Puntos, Describir(encontradas), TipoEstafaSugeridoId);
    }

    protected static string Citar(IReadOnlyList<string> frases)
        => string.Join(", ", frases.Select(f => $"\"{f}\""));
}
