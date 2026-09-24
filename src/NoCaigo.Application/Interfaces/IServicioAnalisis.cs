using NoCaigo.Application.Dtos;

namespace NoCaigo.Application.Interfaces;

/// <summary>Casos de uso de análisis que expone la API.</summary>
public interface IServicioAnalisis
{
    Task<ResultadoAnalisisDto> AnalizarAsync(SolicitudAnalisis solicitud, CancellationToken ct = default);

    Task<ResultadoAnalisisDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

    Task<ResultadoPaginado<AnalisisResumenDto>> ListarAsync(FiltroAnalisis filtro, CancellationToken ct = default);
}
