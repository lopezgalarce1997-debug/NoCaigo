using NoCaigo.Application.Dtos;
using NoCaigo.Domain.Entidades;

namespace NoCaigo.Application.Interfaces;

/// <summary>
/// Acceso a datos de los análisis. Se define en Application y se implementa en
/// Infrastructure (inversión de dependencias): el caso de uso no conoce EF Core.
/// </summary>
public interface IRepositorioAnalisis
{
    /// <summary>Guarda el análisis y deja cargado su tipo de estafa.</summary>
    Task AgregarAsync(Analisis analisis, CancellationToken ct = default);

    /// <summary>Devuelve el análisis con sus señales y tipo, o null si no existe.</summary>
    Task<Analisis?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Página de análisis más recientes primero, y el total que cumple el filtro.</summary>
    Task<(IReadOnlyList<Analisis> Items, int Total)> ListarAsync(FiltroAnalisis filtro, CancellationToken ct = default);
}
