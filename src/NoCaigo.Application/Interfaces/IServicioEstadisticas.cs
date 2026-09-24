using NoCaigo.Application.Dtos;

namespace NoCaigo.Application.Interfaces;

public interface IServicioEstadisticas
{
    Task<IReadOnlyList<EstadisticaPorTipoDto>> ObtenerPorTipoAsync(FiltroFechas filtro, CancellationToken ct = default);

    /// <summary>Las últimas <paramref name="semanas"/> semanas ISO, incluida la actual, en orden cronológico.</summary>
    Task<IReadOnlyList<TendenciaSemanalDto>> ObtenerTendenciaAsync(int semanas, CancellationToken ct = default);
}
