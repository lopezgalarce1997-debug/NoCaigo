namespace NoCaigo.Application.Interfaces;

/// <summary>Consultas de agregación sobre los análisis guardados.</summary>
public interface IRepositorioEstadisticas
{
    /// <summary>
    /// Cantidad de análisis por cada tipo del catálogo, incluidos los que tienen 0.
    /// Los límites son opcionales: desde incluido, hasta excluido (UTC).
    /// </summary>
    Task<IReadOnlyList<ConteoPorTipo>> ContarPorTipoAsync(
        DateTime? desdeUtc, DateTime? hastaExclusivoUtc, CancellationToken ct = default);

    /// <summary>Cantidad de análisis por día (UTC), solo los días que tienen alguno.</summary>
    Task<IReadOnlyList<ConteoPorDia>> ContarPorDiaAsync(
        DateTime desdeUtc, DateTime hastaExclusivoUtc, CancellationToken ct = default);
}

public sealed record ConteoPorTipo(int TipoEstafaId, string Nombre, int Cantidad);

public sealed record ConteoPorDia(DateOnly Dia, int Cantidad);
