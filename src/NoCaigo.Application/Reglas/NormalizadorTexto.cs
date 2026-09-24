using System.Globalization;
using System.Text;

namespace NoCaigo.Application.Reglas;

/// <summary>
/// Normaliza el texto para comparar sin importar mayúsculas ni tildes
/// ("ÚLTIMO aviso" y "ultimo aviso" deben activar la misma regla).
/// </summary>
internal static class NormalizadorTexto
{
    /// <summary>
    /// Devuelve el texto en minúsculas y sin tildes. Para texto en español en forma
    /// compuesta (FormC) conserva el largo, así las posiciones encontradas en el texto
    /// normalizado sirven para recortar el texto original y mostrarlo con sus tildes.
    /// </summary>
    public static string Normalizar(string texto)
    {
        var descompuesto = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(descompuesto.Length);

        foreach (var c in descompuesto)
        {
            // En FormD "á" se separa en "a" + tilde combinable; se descarta la tilde.
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
