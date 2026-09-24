using System.ComponentModel.DataAnnotations;
using NoCaigo.Domain.Enums;

namespace NoCaigo.Application.Dtos;

/// <summary>Filtros y paginación del listado de análisis (llegan por query string).</summary>
public sealed class FiltroAnalisis
{
    public const int TamanoPaginaMaximo = 50;

    /// <summary>Id del tipo de estafa (1 = Falso banco ... 8 = Ninguno).</summary>
    public int? Tipo { get; init; }

    public Canal? Canal { get; init; }

    /// <summary>Fecha inicial (UTC), incluida. Formato yyyy-MM-dd.</summary>
    public DateOnly? Desde { get; init; }

    /// <summary>Fecha final (UTC), incluida completa. Formato yyyy-MM-dd.</summary>
    public DateOnly? Hasta { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "La página debe ser 1 o mayor.")]
    public int Pagina { get; init; } = 1;

    [Range(1, TamanoPaginaMaximo, ErrorMessage = "El tamaño de página debe estar entre 1 y 50.")]
    public int TamanoPagina { get; init; } = 20;
}

public sealed record ResultadoPaginado<T>(IReadOnlyList<T> Items, int Pagina, int TamanoPagina, int Total)
{
    public int TotalPaginas => (int)Math.Ceiling(Total / (double)TamanoPagina);
}
