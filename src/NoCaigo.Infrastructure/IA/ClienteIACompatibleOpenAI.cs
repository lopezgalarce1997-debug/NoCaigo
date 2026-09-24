using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NoCaigo.Application.Interfaces;

namespace NoCaigo.Infrastructure.IA;

/// <summary>
/// Un solo cliente para cualquier proveedor con API compatible con OpenAI
/// (POST {UrlBase}/chat/completions). Ollama y Groq la exponen, así que cambiar
/// de proveedor es solo cambiar la configuración, sin tocar código.
/// </summary>
/// <remarks>
/// El HttpClient llega configurado desde IHttpClientFactory (URL base, timeout y API key),
/// lo que evita el agotamiento de sockets de crear un HttpClient por petición.
/// </remarks>
public sealed class ClienteIACompatibleOpenAI(
    HttpClient http,
    IOptions<OpcionesIA> opciones,
    ILogger<ClienteIACompatibleOpenAI> logger) : IServicioIA
{
    // La API de OpenAI usa snake_case (response_format); nuestras clases, PascalCase.
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<RespuestaIA> AnalizarAsync(string textoAnonimizado, CancellationToken ct = default)
    {
        var proveedor = opciones.Value.ProveedorActivo;
        var modelo = opciones.Value.ObtenerProveedorActivo()?.Modelo ?? string.Empty;

        var solicitud = new SolicitudChat(
            modelo,
            [
                new MensajeChat("system", PromptAnalisis.Sistema),
                new MensajeChat("user", PromptAnalisis.Usuario(textoAnonimizado))
            ],
            Temperature: 0, // Respuestas deterministas: mismo mensaje, mismo análisis.
            ResponseFormat: new FormatoRespuesta("json_object")); // Obliga al modelo a devolver JSON.

        HttpResponseMessage respuesta;
        try
        {
            respuesta = await http.PostAsJsonAsync("chat/completions", solicitud, Json, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw; // Lo canceló quien llamó (por ejemplo, el cliente cerró la conexión): no es una falla de la IA.
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // TaskCanceledException sin cancelación del llamador = se agotó HttpClient.Timeout.
            var motivo = ex is TaskCanceledException ? "no respondió a tiempo" : "no está disponible";
            logger.LogWarning(ex, "El proveedor de IA {Proveedor} {Motivo}", proveedor, motivo);
            throw new ExcepcionServicioIA($"El proveedor de IA {motivo}.", ex);
        }

        using (respuesta)
        {
            if (!respuesta.IsSuccessStatusCode)
            {
                var motivo = respuesta.StatusCode switch
                {
                    HttpStatusCode.TooManyRequests => "agotó la cuota o el límite de peticiones",
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "rechazó la API key",
                    HttpStatusCode.NotFound => "no encontró el modelo o la URL",
                    _ => $"respondió con error {(int)respuesta.StatusCode}"
                };
                logger.LogWarning("El proveedor de IA {Proveedor} {Motivo}", proveedor, motivo);
                throw new ExcepcionServicioIA($"El proveedor de IA {motivo}.");
            }

            RespuestaChat? cuerpo;
            try
            {
                cuerpo = await respuesta.Content.ReadFromJsonAsync<RespuestaChat>(Json, ct);
            }
            catch (JsonException ex)
            {
                throw new ExcepcionServicioIA("El proveedor de IA devolvió una respuesta ilegible.", ex);
            }

            var contenido = cuerpo?.Choices?.FirstOrDefault()?.Message?.Content;
            return ParserRespuestaIA.Parsear(contenido);
        }
    }

    // Solo las partes del formato de OpenAI que se usan.
    private sealed record SolicitudChat(
        string Model, IReadOnlyList<MensajeChat> Messages, double Temperature, FormatoRespuesta ResponseFormat);

    private sealed record MensajeChat(string Role, string Content);

    private sealed record FormatoRespuesta(string Type);

    private sealed record RespuestaChat(IReadOnlyList<OpcionChat>? Choices);

    private sealed record OpcionChat(MensajeChat? Message);
}
