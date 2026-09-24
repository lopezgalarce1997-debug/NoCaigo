namespace NoCaigo.Application.Reglas;

/// <summary>Detecta pedidos de claves, códigos, coordenadas o datos de tarjeta.</summary>
public sealed class ReglaPedidoDatosSensibles() : ReglaPorPalabrasClave(
[
    // Las frases largas van primero: la alternancia se queda con la primera que calce.
    "tarjeta de coordenadas",
    "clave dinamica",
    "codigo (?:de )?(?:verificacion|seguridad|acceso|sms)",
    "(?:datos|numero) de (?:tu|su) tarjeta",
    "coordenadas",
    "superclave",
    "claves?",
    "contrasenas?",
    "pin",
    "cvv",
])
{
    public const int PuntosRiesgo = 35;

    protected override int Puntos => PuntosRiesgo;

    protected override string Describir(IReadOnlyList<string> encontradas)
        => $"Pide datos secretos ({Citar(encontradas)}). Ningún banco ni empresa seria te los pedirá por mensaje.";
}
