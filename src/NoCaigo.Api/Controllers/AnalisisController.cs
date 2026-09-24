using Microsoft.AspNetCore.Mvc;
using NoCaigo.Application.Dtos;
using NoCaigo.Application.Interfaces;

namespace NoCaigo.Api.Controllers;

/// <summary>Analiza mensajes sospechosos y consulta análisis anteriores.</summary>
// [ApiController] valida el modelo automáticamente: si SolicitudAnalisis no cumple
// sus DataAnnotations, responde 400 con los errores sin llegar a la acción.
[ApiController]
[Route("api/analisis")]
[Produces("application/json")]
public sealed class AnalisisController(IServicioAnalisis servicio) : ControllerBase
{
    /// <summary>Analiza un mensaje y guarda el resultado.</summary>
    /// <remarks>Los datos personales (teléfonos, RUT, correos, tarjetas) se anonimizan antes de guardar.</remarks>
    [HttpPost]
    [ProducesResponseType<ResultadoAnalisisDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Analizar(SolicitudAnalisis solicitud, CancellationToken ct)
    {
        var resultado = await servicio.AnalizarAsync(solicitud, ct);

        // 201 + cabecera Location apuntando al detalle: la convención REST al crear un recurso.
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
    }

    /// <summary>Devuelve el detalle de un análisis.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ResultadoAnalisisDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id, CancellationToken ct)
    {
        var resultado = await servicio.ObtenerPorIdAsync(id, ct);
        return resultado is null ? NotFound() : Ok(resultado);
    }

    /// <summary>Lista análisis con filtros opcionales y paginación (más recientes primero).</summary>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginado<AnalisisResumenDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar([FromQuery] FiltroAnalisis filtro, CancellationToken ct)
        => Ok(await servicio.ListarAsync(filtro, ct));
}
