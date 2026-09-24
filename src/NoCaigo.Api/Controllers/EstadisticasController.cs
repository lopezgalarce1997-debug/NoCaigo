using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using NoCaigo.Application.Dtos;
using NoCaigo.Application.Interfaces;
using NoCaigo.Application.Servicios;

namespace NoCaigo.Api.Controllers;

/// <summary>Estadísticas agregadas de los análisis guardados.</summary>
[ApiController]
[Route("api/estadisticas")]
[Produces("application/json")]
public sealed class EstadisticasController(IServicioEstadisticas servicio) : ControllerBase
{
    /// <summary>Cantidad y porcentaje de análisis por tipo de estafa (todos los tipos, de mayor a menor).</summary>
    [HttpGet("por-tipo")]
    [ProducesResponseType<IReadOnlyList<EstadisticaPorTipoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PorTipo([FromQuery] FiltroFechas filtro, CancellationToken ct)
        => Ok(await servicio.ObtenerPorTipoAsync(filtro, ct));

    /// <summary>Cantidad de análisis por semana ISO 8601 (lunes a domingo, UTC), incluidas las semanas sin análisis.</summary>
    /// <param name="semanas">Cuántas semanas hacia atrás, incluida la actual (1 a 52).</param>
    /// <param name="ct">Cancelación de la petición.</param>
    [HttpGet("tendencia")]
    [ProducesResponseType<IReadOnlyList<TendenciaSemanalDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Tendencia(
        [FromQuery, Range(1, ServicioEstadisticas.SemanasMaximas, ErrorMessage = "semanas debe estar entre 1 y 52.")]
        int semanas = 12,
        CancellationToken ct = default)
        => Ok(await servicio.ObtenerTendenciaAsync(semanas, ct));
}
