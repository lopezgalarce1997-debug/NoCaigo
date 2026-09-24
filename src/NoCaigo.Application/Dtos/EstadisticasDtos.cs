using System.ComponentModel.DataAnnotations;

namespace NoCaigo.Application.Dtos;

/// <summary>Rango de fechas opcional (UTC, ambos extremos incluidos).</summary>
public sealed class FiltroFechas : IValidatableObject
{
    /// <summary>Fecha inicial (UTC), incluida. Formato yyyy-MM-dd.</summary>
    public DateOnly? Desde { get; init; }

    /// <summary>Fecha final (UTC), incluida completa. Formato yyyy-MM-dd.</summary>
    public DateOnly? Hasta { get; init; }

    // [ApiController] ejecuta esta validación y responde 400 si falla, igual que con los atributos.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Desde > Hasta)
            yield return new ValidationResult("'desde' no puede ser posterior a 'hasta'.", [nameof(Desde), nameof(Hasta)]);
    }
}

/// <summary>Cantidad de análisis de un tipo de estafa y su porcentaje sobre el total.</summary>
public sealed record EstadisticaPorTipoDto(int TipoEstafaId, string TipoEstafa, int Cantidad, double Porcentaje);

/// <summary>Cantidad de análisis en una semana ISO 8601 (de lunes a domingo, UTC).</summary>
/// <param name="Semana">Identificador ISO, por ejemplo "2026-W39".</param>
public sealed record TendenciaSemanalDto(string Semana, DateOnly Inicio, DateOnly Fin, int Cantidad);
