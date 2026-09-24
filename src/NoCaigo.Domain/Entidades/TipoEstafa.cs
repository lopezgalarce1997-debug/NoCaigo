namespace NoCaigo.Domain.Entidades;

/// <summary>
/// Catálogo de tipos de estafa. Es una tabla (y no un enum) para poder
/// agregar tipos nuevos con datos, sin recompilar ni migrar el esquema.
/// </summary>
public class TipoEstafa
{
    // Ids fijos del catálogo semilla, para referenciarlos desde código sin "números mágicos".
    public const int FalsoBanco = 1;
    public const int PaqueteRetenido = 2;
    public const int FalsoFamiliar = 3;
    public const int PremioFalso = 4;
    public const int FalsaOfertaTrabajo = 5;
    public const int InversionFalsa = 6;
    public const int Otro = 7;
    public const int Ninguno = 8;

    /// <summary>
    /// Catálogo completo. Única fuente de verdad: lo usan los datos semilla de la base
    /// y el prompt/parser de la IA, así ambos nunca quedan desalineados.
    /// </summary>
    public static IReadOnlyList<TipoEstafa> Catalogo { get; } =
    [
        new(FalsoBanco, "Falso banco"),
        new(PaqueteRetenido, "Paquete retenido"),
        new(FalsoFamiliar, "Falso familiar"),
        new(PremioFalso, "Premio falso"),
        new(FalsaOfertaTrabajo, "Falsa oferta de trabajo"),
        new(InversionFalsa, "Inversión falsa"),
        new(Otro, "Otro"),
        new(Ninguno, "Ninguno"),
    ];

    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;

    // Constructor vacío requerido por EF Core para materializar filas.
    private TipoEstafa() { }

    public TipoEstafa(int id, string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del tipo de estafa es obligatorio.", nameof(nombre));

        Id = id;
        Nombre = nombre;
    }
}
