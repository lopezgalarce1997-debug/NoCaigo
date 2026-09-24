namespace NoCaigo.Application.Servicios;

/// <summary>Sección "Combinacion" de appsettings: pesos del promedio entre reglas e IA.</summary>
public sealed class OpcionesCombinacion
{
    public const string Seccion = "Combinacion";

    public double PesoReglas { get; set; } = 0.4;

    public double PesoIA { get; set; } = 0.6;

    /// <summary>Cada peso entre 0 y 1 y que sumen 1 (con tolerancia por decimales como 0.1 + 0.2).</summary>
    public bool EsValida()
        => PesoReglas is >= 0 and <= 1
           && PesoIA is >= 0 and <= 1
           && Math.Abs(PesoReglas + PesoIA - 1) < 0.0001;
}
