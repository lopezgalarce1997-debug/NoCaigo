using Microsoft.EntityFrameworkCore;
using NoCaigo.Application.Dtos;
using NoCaigo.Application.Interfaces;
using NoCaigo.Domain.Entidades;

namespace NoCaigo.Infrastructure.Persistencia.Repositorios;

public sealed class RepositorioAnalisis(NoCaigoDbContext db) : IRepositorioAnalisis
{
    public async Task AgregarAsync(Analisis analisis, CancellationToken ct = default)
    {
        db.Analisis.Add(analisis);
        await db.SaveChangesAsync(ct);

        // Carga el nombre del tipo para que quien llama pueda devolverlo sin otra consulta manual.
        await db.Entry(analisis).Reference(a => a.TipoEstafa).LoadAsync(ct);
    }

    public Task<Analisis?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        => db.Analisis
            .AsNoTracking() // Solo lectura: EF no necesita seguir cambios.
            .Include(a => a.TipoEstafa)
            .Include(a => a.Senales)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<(IReadOnlyList<Analisis> Items, int Total)> ListarAsync(
        FiltroAnalisis filtro, CancellationToken ct = default)
    {
        // IQueryable: cada filtro se agrega a la consulta y todo se ejecuta como un solo SQL.
        var consulta = db.Analisis.AsNoTracking();

        if (filtro.Tipo is not null)
            consulta = consulta.Where(a => a.TipoEstafaId == filtro.Tipo);

        if (filtro.Canal is not null)
            consulta = consulta.Where(a => a.Canal == filtro.Canal);

        if (filtro.Desde is not null)
        {
            var desde = filtro.Desde.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            consulta = consulta.Where(a => a.FechaCreacion >= desde);
        }

        if (filtro.Hasta is not null)
        {
            // "Hasta" incluye el día completo: se compara con el inicio del día siguiente.
            var hastaExclusivo = filtro.Hasta.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            consulta = consulta.Where(a => a.FechaCreacion < hastaExclusivo);
        }

        var total = await consulta.CountAsync(ct);

        var items = await consulta
            .Include(a => a.TipoEstafa)
            .OrderByDescending(a => a.FechaCreacion)
            .ThenByDescending(a => a.Id) // Orden estable para que la paginación no repita filas.
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .ToListAsync(ct);

        return (items, total);
    }
}
