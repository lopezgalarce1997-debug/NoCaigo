using NoCaigo.Domain.Enums;

namespace NoCaigo.Domain.Entidades;

/// <summary>Indicio concreto encontrado en el mensaje, explicado en lenguaje simple.</summary>
public class Senal
{
    public int Id { get; private set; }
    public int AnalisisId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public OrigenSenal Origen { get; private set; }

    private Senal() { }

    // internal: solo Analisis (dentro de Domain) puede crear señales,
    // así una señal nunca existe sin su análisis.
    internal Senal(string descripcion, OrigenSenal origen)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException("La descripción de la señal es obligatoria.", nameof(descripcion));

        Descripcion = descripcion;
        Origen = origen;
    }
}
