namespace NoCaigo.Application.Interfaces;

/// <summary>
/// Regla determinística de detección (patrón Strategy): cada implementación
/// busca un tipo de señal distinto y el caso de uso las ejecuta todas por igual.
/// </summary>
public interface IReglaDeteccion
{
    ResultadoRegla Evaluar(string texto);
}

/// <param name="Activada">Si la regla encontró la señal en el texto.</param>
/// <param name="Puntos">Puntos de riesgo que suma (0 si no se activó).</param>
/// <param name="Descripcion">Explicación en lenguaje simple para la persona.</param>
/// <param name="TipoEstafaSugeridoId">
/// Tipo de estafa que la señal sugiere (por ejemplo, un dominio que imita a un banco
/// sugiere "Falso banco"). Null si la señal es genérica.
/// </param>
public sealed record ResultadoRegla(
    bool Activada,
    int Puntos,
    string Descripcion,
    int? TipoEstafaSugeridoId = null)
{
    public static readonly ResultadoRegla NoActivada = new(false, 0, string.Empty);
}
