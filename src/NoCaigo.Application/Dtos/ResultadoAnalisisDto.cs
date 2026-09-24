using NoCaigo.Domain.Entidades;
using NoCaigo.Domain.Enums;

namespace NoCaigo.Application.Dtos;

/// <summary>Detalle completo de un análisis, tal como lo ve quien consulta la API.</summary>
public sealed record ResultadoAnalisisDto(
    int Id,
    Veredicto Veredicto,
    int NivelRiesgo,
    string TipoEstafa,
    IReadOnlyList<SenalDto> Senales,
    string Explicacion,
    bool UsoIA,
    Canal Canal,
    string TextoAnonimizado,
    DateTime FechaCreacion)
{
    // Mapeo manual en vez de AutoMapper: es explícito, se depura fácil y
    // un cambio en la entidad rompe la compilación en vez de fallar en ejecución.
    public static ResultadoAnalisisDto DesdeEntidad(Analisis analisis) => new(
        analisis.Id,
        analisis.Veredicto,
        analisis.NivelRiesgo,
        analisis.TipoEstafa?.Nombre ?? string.Empty,
        analisis.Senales.Select(s => new SenalDto(s.Descripcion, s.Origen)).ToList(),
        analisis.Explicacion,
        analisis.UsoIA,
        analisis.Canal,
        analisis.TextoAnonimizado,
        analisis.FechaCreacion);
}

public sealed record SenalDto(string Descripcion, OrigenSenal Origen);

/// <summary>Versión reducida para el listado (sin texto ni señales).</summary>
public sealed record AnalisisResumenDto(
    int Id,
    Veredicto Veredicto,
    int NivelRiesgo,
    string TipoEstafa,
    Canal Canal,
    bool UsoIA,
    DateTime FechaCreacion)
{
    public static AnalisisResumenDto DesdeEntidad(Analisis analisis) => new(
        analisis.Id,
        analisis.Veredicto,
        analisis.NivelRiesgo,
        analisis.TipoEstafa?.Nombre ?? string.Empty,
        analisis.Canal,
        analisis.UsoIA,
        analisis.FechaCreacion);
}
