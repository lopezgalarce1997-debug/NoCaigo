using NoCaigo.Domain.Enums;

namespace NoCaigo.Domain.Entidades;

/// <summary>
/// Resultado de analizar un mensaje. Es la raíz del agregado: las señales
/// solo se agregan a través de él, y valida sus propias invariantes.
/// </summary>
public class Analisis
{
    public const int NivelRiesgoMinimo = 0;
    public const int NivelRiesgoMaximo = 100;

    private readonly List<Senal> _senales = [];

    public int Id { get; private set; }
    public string TextoAnonimizado { get; private set; } = string.Empty;
    public Canal Canal { get; private set; }
    public int NivelRiesgo { get; private set; }
    public Veredicto Veredicto { get; private set; }
    public int TipoEstafaId { get; private set; }
    public TipoEstafa? TipoEstafa { get; private set; }
    public string Explicacion { get; private set; } = string.Empty;
    public bool UsoIA { get; private set; }
    public DateTime FechaCreacion { get; private set; }

    // Se expone como solo lectura para que nadie modifique la lista desde fuera.
    public IReadOnlyCollection<Senal> Senales => _senales.AsReadOnly();

    private Analisis() { }

    public Analisis(
        string textoAnonimizado,
        Canal canal,
        int nivelRiesgo,
        Veredicto veredicto,
        int tipoEstafaId,
        string explicacion,
        bool usoIA,
        DateTime fechaCreacionUtc)
    {
        if (string.IsNullOrWhiteSpace(textoAnonimizado))
            throw new ArgumentException("El texto del análisis es obligatorio.", nameof(textoAnonimizado));

        if (nivelRiesgo is < NivelRiesgoMinimo or > NivelRiesgoMaximo)
            throw new ArgumentOutOfRangeException(nameof(nivelRiesgo),
                $"El nivel de riesgo debe estar entre {NivelRiesgoMinimo} y {NivelRiesgoMaximo}.");

        if (fechaCreacionUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("La fecha de creación debe estar en UTC.", nameof(fechaCreacionUtc));

        TextoAnonimizado = textoAnonimizado;
        Canal = canal;
        NivelRiesgo = nivelRiesgo;
        Veredicto = veredicto;
        TipoEstafaId = tipoEstafaId;
        Explicacion = explicacion ?? string.Empty;
        UsoIA = usoIA;
        FechaCreacion = fechaCreacionUtc;
    }

    public void AgregarSenal(string descripcion, OrigenSenal origen)
        => _senales.Add(new Senal(descripcion, origen));
}
