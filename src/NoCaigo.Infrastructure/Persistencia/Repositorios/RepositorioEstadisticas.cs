using Microsoft.EntityFrameworkCore;
using NoCaigo.Application.Interfaces;

namespace NoCaigo.Infrastructure.Persistencia.Repositorios;

public sealed class RepositorioEstadisticas(NoCaigoDbContext db) : IRepositorioEstadisticas
{
    public async Task<IReadOnlyList<ConteoPorTipo>> ContarPorTipoAsync(
        DateTime? desdeUtc, DateTime? hastaExclusivoUtc, CancellationToken ct = default)
    {
        var analisis = db.Analisis.AsQueryable();

        if (desdeUtc is not null)
            analisis = analisis.Where(a => a.FechaCreacion >= desdeUtc);

        if (hastaExclusivoUtc is not null)
            analisis = analisis.Where(a => a.FechaCreacion < hastaExclusivoUtc);

        // Se parte del catálogo (no de los análisis) para incluir los tipos con 0.
        // EF lo traduce a un solo SELECT con una subconsulta COUNT por tipo.
        return await db.TiposEstafa
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .Select(t => new ConteoPorTipo(t.Id, t.Nombre, analisis.Count(a => a.TipoEstafaId == t.Id)))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ConteoPorDia>> ContarPorDiaAsync(
        DateTime desdeUtc, DateTime hastaExclusivoUtc, CancellationToken ct = default)
    {
        // GROUP BY CONVERT(date, FechaCreacion): la base devuelve una fila por día, no cada análisis.
        var filas = await db.Analisis
            .Where(a => a.FechaCreacion >= desdeUtc && a.FechaCreacion < hastaExclusivoUtc)
            .GroupBy(a => a.FechaCreacion.Date)
            .Select(g => new { Dia = g.Key, Cantidad = g.Count() })
            .ToListAsync(ct);

        return filas.Select(f => new ConteoPorDia(DateOnly.FromDateTime(f.Dia), f.Cantidad)).ToList();
    }
}
