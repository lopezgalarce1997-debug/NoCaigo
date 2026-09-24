namespace NoCaigo.Application.Reglas;

/// <summary>Detecta presión de tiempo o amenazas para que la persona actúe sin pensar.</summary>
public sealed class ReglaUrgencia() : ReglaPorPalabrasClave(
[
    "hoy",
    "ultimo aviso",
    "urgente",
    "inmediatamente",
    "de inmediato",
    "bloquead[oa]s?",
    "suspendid[oa]s?",
    "\\d{1,2} horas",
    "expira",
    "de lo contrario",
    "a la brevedad",
])
{
    public const int PuntosRiesgo = 15;

    protected override int Puntos => PuntosRiesgo;

    protected override string Describir(IReadOnlyList<string> encontradas)
        => $"Usa palabras de urgencia ({Citar(encontradas)}) para presionarte a actuar rápido y sin verificar.";
}
