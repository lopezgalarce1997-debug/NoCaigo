using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoCaigo.Domain.Entidades;

namespace NoCaigo.Infrastructure.Persistencia.Configuraciones;

public class SenalConfiguracion : IEntityTypeConfiguration<Senal>
{
    public void Configure(EntityTypeBuilder<Senal> builder)
    {
        builder.ToTable("Senales");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Descripcion).HasMaxLength(500).IsRequired();
        builder.Property(s => s.Origen).HasConversion<string>().HasMaxLength(10);
    }
}
