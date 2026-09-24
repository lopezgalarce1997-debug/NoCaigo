using NoCaigo.Domain.Enums;

namespace NoCaigo.Domain.Reglas;

/// <summary>
/// Regla de negocio que traduce un nivel de riesgo (0-100) a un veredicto.
/// Vive en Domain porque no depende de cómo se calculó el riesgo (reglas o IA).
/// </summary>
public static class ClasificadorRiesgo
{
    /// <summary>Desde este nivel el mensaje se considera sospechoso.</summary>
    public const int UmbralSospechoso = 25;

    /// <summary>Desde este nivel el mensaje se considera estafa.</summary>
    public const int UmbralEstafa = 60;

    public static Veredicto ObtenerVeredicto(int nivelRiesgo) => nivelRiesgo switch
    {
        >= UmbralEstafa => Veredicto.Estafa,
        >= UmbralSospechoso => Veredicto.Sospechoso,
        _ => Veredicto.Seguro
    };
}
