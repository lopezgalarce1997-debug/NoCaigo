using NoCaigo.Application.Dtos;
using NoCaigo.Application.Interfaces;

namespace NoCaigo.Application.Servicios;

public sealed class ServicioEstadisticas(IRepositorioEstadisticas repositorio, TimeProvider reloj) : IServicioEstadisticas
{
    public const int SemanasMaximas = 52;

    public async Task<IReadOnlyList<EstadisticaPorTipoDto>> ObtenerPorTipoAsync(
        FiltroFechas filtro, CancellationToken ct = default)
    {
        var conteos = await repositorio.ContarPorTipoAsync(
            filtro.Desde?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            // "Hasta" incluye el día completo: se pasa el inicio del día siguiente como límite excluido.
            filtro.Hasta?.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            ct);

        var total = conteos.Sum(c => c.Cantidad);

        return conteos
            .Select(c => new EstadisticaPorTipoDto(
                c.TipoEstafaId,
                c.Nombre,
                c.Cantidad,
                total == 0 ? 0 : Math.Round(100.0 * c.Cantidad / total, 1)))
            .OrderByDescending(e => e.Cantidad)
            .ThenBy(e => e.TipoEstafaId)
            .ToList();
    }

    public async Task<IReadOnlyList<TendenciaSemanalDto>> ObtenerTendenciaAsync(int semanas, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(semanas, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(semanas, SemanasMaximas);

        var hoy = DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime);
        var inicioSemanaActual = SemanaIso.Inicio(hoy);
        var desde = inicioSemanaActual.AddDays(-7 * (semanas - 1));
        var hastaExclusivo = inicioSemanaActual.AddDays(7);

        // SQL Server no conoce las semanas ISO sin trucos dependientes de SET DATEFIRST.
        // Se agrupa por DÍA en la base (a lo más 7 × 52 filas) y por semana aquí, en C#,
        // con ISOWeek de .NET: correcto, portable y fácil de probar.
        var conteosPorDia = await repositorio.ContarPorDiaAsync(
            desde.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            hastaExclusivo.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            ct);

        var cantidadPorSemana = conteosPorDia
            .GroupBy(c => SemanaIso.Inicio(c.Dia))
            .ToDictionary(g => g.Key, g => g.Sum(c => c.Cantidad));

        // Se devuelven TODAS las semanas del rango, también las que tienen 0:
        // un gráfico de tendencia sin huecos no engaña a quien lo lee.
        return Enumerable.Range(0, semanas)
            .Select(i => desde.AddDays(7 * i))
            .Select(inicio => new TendenciaSemanalDto(
                SemanaIso.Etiqueta(inicio),
                inicio,
                inicio.AddDays(6),
                cantidadPorSemana.GetValueOrDefault(inicio)))
            .ToList();
    }
}
