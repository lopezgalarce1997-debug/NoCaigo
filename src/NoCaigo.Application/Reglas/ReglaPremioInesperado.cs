using NoCaigo.Domain.Entidades;

namespace NoCaigo.Application.Reglas;

/// <summary>Detecta premios, sorteos o beneficios que la persona no esperaba.</summary>
public sealed class ReglaPremioInesperado() : ReglaPorPalabrasClave(
[
    "ganaste",
    "(?:has|ha) ganado",
    "ganador(?:a)?",
    "premios?",
    "sorteo",
    "felicidades",
    "felicitaciones",
    "(?:fuiste|has sido) seleccionad[oa]",
    "reclama tu",
    "bono",
])
{
    public const int PuntosRiesgo = 25;

    protected override int Puntos => PuntosRiesgo;

    protected override int? TipoEstafaSugeridoId => TipoEstafa.PremioFalso;

    protected override string Describir(IReadOnlyList<string> encontradas)
        => $"Promete un premio o beneficio inesperado ({Citar(encontradas)}). Si no participaste en nada, desconfía.";
}
