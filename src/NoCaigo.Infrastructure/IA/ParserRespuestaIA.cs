using System.Text.Json;
using NoCaigo.Application.Interfaces;
using NoCaigo.Application.Reglas;
using NoCaigo.Domain.Entidades;
using NoCaigo.Domain.Enums;
using NoCaigo.Domain.Reglas;

namespace NoCaigo.Infrastructure.IA;

/// <summary>
/// Convierte el texto que devuelve el modelo en una <see cref="RespuestaIA"/> válida.
/// Nunca confía en el modelo: lo único obligatorio es un nivelRiesgo numérico; el resto
/// se valida, se corrige o se descarta. Si no se puede parsear, lanza <see cref="ExcepcionServicioIA"/>.
/// </summary>
public static class ParserRespuestaIA
{
    public const int MaximoSenales = 5;
    public const int LargoMaximoSenal = 500;       // Igual al largo de la columna Senales.Descripcion
    public const int LargoMaximoExplicacion = 2000; // Igual al largo de la columna Analisis.Explicacion

    // Nombre normalizado (sin tildes ni mayúsculas) → Id del catálogo.
    private static readonly Dictionary<string, int> TiposPorNombre = TipoEstafa.Catalogo
        .ToDictionary(t => NormalizadorTexto.Normalizar(t.Nombre), t => t.Id);

    public static RespuestaIA Parsear(string? contenido)
    {
        var json = ExtraerObjetoJson(contenido)
            ?? throw new ExcepcionServicioIA("La IA no devolvió un objeto JSON.");

        JsonDocument documento;
        try
        {
            documento = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new ExcepcionServicioIA("La IA devolvió un JSON mal formado.", ex);
        }

        using (documento)
        {
            var raiz = documento.RootElement;
            if (raiz.ValueKind != JsonValueKind.Object)
                throw new ExcepcionServicioIA("La respuesta de la IA no es un objeto JSON.");

            var nivelRiesgo = LeerNivelRiesgo(raiz)
                ?? throw new ExcepcionServicioIA("La respuesta de la IA no trae un nivelRiesgo numérico.");

            return new RespuestaIA(
                LeerTipo(raiz),
                nivelRiesgo,
                LeerVeredicto(raiz) ?? ClasificadorRiesgo.ObtenerVeredicto(nivelRiesgo),
                LeerSenales(raiz),
                Recortar(LeerTexto(raiz, "explicacion") ?? string.Empty, LargoMaximoExplicacion));
        }
    }

    // Los modelos pequeños a veces envuelven el JSON en ```json ... ``` o agregan texto antes/después.
    // Se toma desde la primera '{' hasta la última '}'.
    private static string? ExtraerObjetoJson(string? contenido)
    {
        if (string.IsNullOrWhiteSpace(contenido))
            return null;

        var inicio = contenido.IndexOf('{');
        var fin = contenido.LastIndexOf('}');
        return inicio >= 0 && fin > inicio ? contenido[inicio..(fin + 1)] : null;
    }

    private static int? LeerNivelRiesgo(JsonElement raiz)
    {
        if (!raiz.TryGetProperty("nivelRiesgo", out var valor))
            return null;

        double? numero = valor.ValueKind switch
        {
            JsonValueKind.Number => valor.GetDouble(),
            // Algunos modelos devuelven el número entre comillas: "85".
            JsonValueKind.String when double.TryParse(valor.GetString(),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var n) => n,
            _ => null
        };

        if (numero is null || double.IsNaN(numero.Value))
            return null;

        // Fuera de rango se ajusta en vez de descartar: 120 claramente significa "riesgo máximo".
        return (int)Math.Clamp(Math.Round(numero.Value), Analisis.NivelRiesgoMinimo, Analisis.NivelRiesgoMaximo);
    }

    private static int LeerTipo(JsonElement raiz)
    {
        var nombre = LeerTexto(raiz, "tipoEstafa");
        if (nombre is null)
            return TipoEstafa.Otro;

        // Un tipo inventado por el modelo ("Phishing") se clasifica como "Otro".
        return TiposPorNombre.TryGetValue(NormalizadorTexto.Normalizar(nombre.Trim()), out var id)
            ? id
            : TipoEstafa.Otro;
    }

    private static Veredicto? LeerVeredicto(JsonElement raiz)
    {
        var texto = LeerTexto(raiz, "veredicto");

        // Enum.TryParse acepta números ("3"); se exige que sea uno de los nombres definidos.
        return texto is not null
               && Enum.TryParse<Veredicto>(texto.Trim(), ignoreCase: true, out var veredicto)
               && Enum.IsDefined(veredicto)
               && !int.TryParse(texto, out _)
            ? veredicto
            : null;
    }

    private static List<string> LeerSenales(JsonElement raiz)
    {
        if (!raiz.TryGetProperty("senales", out var senales) || senales.ValueKind != JsonValueKind.Array)
            return [];

        return senales.EnumerateArray()
            .Where(s => s.ValueKind == JsonValueKind.String)
            .Select(s => s.GetString()!.Trim())
            .Where(s => s.Length > 0)
            .Take(MaximoSenales)
            .Select(s => Recortar(s, LargoMaximoSenal))
            .ToList();
    }

    private static string? LeerTexto(JsonElement raiz, string propiedad)
        => raiz.TryGetProperty(propiedad, out var valor) && valor.ValueKind == JsonValueKind.String
            ? valor.GetString()
            : null;

    private static string Recortar(string texto, int largoMaximo)
        => texto.Length <= largoMaximo ? texto : texto[..(largoMaximo - 1)] + "…";
}
