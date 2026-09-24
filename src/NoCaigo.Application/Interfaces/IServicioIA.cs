using NoCaigo.Domain.Enums;

namespace NoCaigo.Application.Interfaces;

/// <summary>
/// Análisis de un mensaje con un modelo de lenguaje. Application solo conoce este
/// contrato; qué proveedor responde (Ollama, Groq...) se decide en Infrastructure por configuración.
/// </summary>
public interface IServicioIA
{
    /// <param name="textoAnonimizado">Texto ya anonimizado: nunca se envían datos personales.</param>
    /// <exception cref="ExcepcionServicioIA">
    /// Si el proveedor falla, no responde a tiempo, agotó la cuota o devuelve un JSON inválido.
    /// </exception>
    Task<RespuestaIA> AnalizarAsync(string textoAnonimizado, CancellationToken ct = default);
}

/// <summary>Respuesta de la IA ya validada y normalizada.</summary>
public sealed record RespuestaIA(
    int TipoEstafaId,
    int NivelRiesgo,
    Veredicto Veredicto,
    IReadOnlyList<string> Senales,
    string Explicacion);

/// <summary>
/// Cualquier falla de la IA se traduce a esta excepción, así el caso de uso maneja
/// un solo tipo de error sin conocer HttpClient ni detalles del proveedor.
/// </summary>
public sealed class ExcepcionServicioIA(string message, Exception? innerException = null)
    : Exception(message, innerException);
