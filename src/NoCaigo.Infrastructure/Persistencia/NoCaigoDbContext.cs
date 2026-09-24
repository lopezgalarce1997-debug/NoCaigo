using Microsoft.EntityFrameworkCore;
using NoCaigo.Domain.Entidades;

namespace NoCaigo.Infrastructure.Persistencia;

public class NoCaigoDbContext(DbContextOptions<NoCaigoDbContext> options) : DbContext(options)
{
    public DbSet<Analisis> Analisis => Set<Analisis>();
    public DbSet<Senal> Senales => Set<Senal>();
    public DbSet<TipoEstafa> TiposEstafa => Set<TipoEstafa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Cada entidad tiene su propia clase IEntityTypeConfiguration en Configuraciones/,
        // así el mapeo no se amontona aquí y las entidades del dominio quedan sin atributos de EF.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NoCaigoDbContext).Assembly);
    }
}
