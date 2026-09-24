using System.Text.RegularExpressions;
using NoCaigo.Application.Interfaces;

namespace NoCaigo.Application.Reglas;

/// <summary>Detecta links acortados, que esconden la dirección real del sitio.</summary>
public sealed partial class ReglaLinkAcortado : IReglaDeteccion
{
    public const int PuntosRiesgo = 25;

    public ResultadoRegla Evaluar(string texto)
    {
        ArgumentNullException.ThrowIfNull(texto);

        var coincidencia = RegexAcortador().Match(texto);

        return coincidencia.Success
            ? new ResultadoRegla(true, PuntosRiesgo,
                $"Incluye un link acortado ({coincidencia.Value}): este tipo de enlace oculta la página real a la que te lleva.")
            : ResultadoRegla.NoActivada;
    }

    // Acortadores más usados. El \b inicial evita calzar "t.co" dentro de "chat.com".
    [GeneratedRegex(
        @"\b(?:bit\.ly|tinyurl\.com|t\.co|goo\.gl|ow\.ly|is\.gd|cutt\.ly|rebrand\.ly|shorturl\.at|rb\.gy|t\.ly|tiny\.cc|acortar\.link|s\.id)(?:/\S*)?",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, 200)]
    private static partial Regex RegexAcortador();
}
