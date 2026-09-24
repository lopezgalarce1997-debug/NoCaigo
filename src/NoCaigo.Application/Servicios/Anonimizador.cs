using System.Text.RegularExpressions;
using NoCaigo.Application.Interfaces;

namespace NoCaigo.Application.Servicios;

/// <summary>
/// Anonimiza correos, tarjetas, RUT chilenos y teléfonos chilenos usando expresiones regulares.
/// Prioriza la privacidad: ante la duda, reemplaza (no valida Luhn ni dígito verificador).
/// </summary>
public partial class Anonimizador : IAnonimizador
{
    public const string MarcadorCorreo = "[CORREO]";
    public const string MarcadorTarjeta = "[TARJETA]";
    public const string MarcadorRut = "[RUT]";
    public const string MarcadorTelefono = "[TELEFONO]";

    // Tiempo máximo por expresión: evita que un texto malicioso cuelgue el servidor (ReDoS).
    private const int TimeoutMs = 200;

    public string Anonimizar(string texto)
    {
        ArgumentNullException.ThrowIfNull(texto);

        // El orden importa: primero los patrones más largos o específicos, para que
        // por ejemplo los dígitos de una tarjeta no se confundan con un teléfono.
        texto = RegexCorreo().Replace(texto, MarcadorCorreo);
        texto = RegexTarjeta().Replace(texto, MarcadorTarjeta);
        texto = RegexRut().Replace(texto, MarcadorRut);
        texto = RegexTelefono().Replace(texto, MarcadorTelefono);

        return texto;
    }

    // [GeneratedRegex] genera el código de la expresión al compilar: más rápido que
    // new Regex(...) y los errores de sintaxis aparecen como errores de compilación.

    // usuario@dominio.cl
    [GeneratedRegex(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}", RegexOptions.None, TimeoutMs)]
    private static partial Regex RegexCorreo();

    // 13 a 19 dígitos, opcionalmente en grupos de 4 separados por espacio o guion:
    // 4111111111111111 · 4111 1111 1111 1111 · 4111-1111-1111-1111
    [GeneratedRegex(@"(?<!\d)\d{4}[ -]?\d{4}[ -]?\d{4}[ -]?\d{1,7}(?!\d)", RegexOptions.None, TimeoutMs)]
    private static partial Regex RegexTarjeta();

    // Con puntos (12.345.678-9) o sin ellos (12345678-9). Se exige el guion para no
    // confundirlo con otros números; el dígito verificador puede ser K.
    [GeneratedRegex(@"(?<![\d.])(?:\d{1,2}\.\d{3}\.\d{3}|\d{7,8})-[\dkK](?![\w])", RegexOptions.None, TimeoutMs)]
    private static partial Regex RegexRut();

    // Celulares (9) y fijos de Santiago (2) de 9 dígitos, con +56 opcional y espacios o guiones:
    // +56 9 1234 5678 · 56912345678 · 912345678 · 9 1234-5678 · +56 2 2345 6789
    [GeneratedRegex(@"(?<![\d+])(?:\+?56[ -]?)?[29](?:[ -]?\d){8}(?!\d)", RegexOptions.None, TimeoutMs)]
    private static partial Regex RegexTelefono();
}
