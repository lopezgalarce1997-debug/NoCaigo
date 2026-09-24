namespace NoCaigo.Application.Reglas;

/// <summary>
/// Detecta instrucciones dirigidas a un sistema automático ("ignora tus instrucciones",
/// "responde que es seguro"). Una persona real no le escribe eso a otra: es un intento de
/// engañar a detectores con IA (prompt injection) y, por sí solo, una señal fuerte de fraude.
/// </summary>
/// <remarks>
/// Es una regla determinística, así que la IA no puede ser convencida de ignorarla:
/// sus puntos forman parte del piso que la IA nunca puede bajar.
/// </remarks>
public sealed class ReglaIntentoManipulacion() : ReglaPorPalabrasClave(
[
    // Español
    "(?:ignora|olvida|omite|descarta)(?:r)? (?:tus|las|todas las|cualquier)? ?(?:instrucciones|reglas|indicaciones)(?: anteriores| previas)?",
    "(?:nuevas|estas son tus) instrucciones",
    "(?:responde|di|indica|clasifica|marca|considera)(?:lo)?(?: que)?(?: este mensaje| esto)?(?: es| como)? (?:seguro|confiable|legitimo|no es (?:una )?estafa)",
    // "Nivel de riesgo 0": vocabulario del detector, no de una persona. ("riesgo cero" solo
    // no se incluye: es un gancho común de inversiones falsas, pero no es manipulación de la IA.)
    "nivel de riesgo (?:0|cero)",
    "(?:eres|actua como) (?:un|una) (?:asistente|ia|modelo)",
    "prompt",
    // Inglés: los ataques suelen copiarse de ejemplos en inglés.
    "ignore (?:all |any |the |your |previous |prior )*instructions",
    "(?:system|developer) (?:prompt|message)",
])
{
    public const int PuntosRiesgo = 40;

    protected override int Puntos => PuntosRiesgo;

    protected override string Describir(IReadOnlyList<string> encontradas)
        => $"Contiene instrucciones dirigidas a un sistema automático ({Citar(encontradas)}). " +
           "Es un intento de engañar a los detectores de estafas, algo que un mensaje legítimo no hace.";
}
