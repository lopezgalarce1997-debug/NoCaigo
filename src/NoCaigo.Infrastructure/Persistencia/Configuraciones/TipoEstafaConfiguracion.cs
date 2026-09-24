using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoCaigo.Domain.Entidades;

namespace NoCaigo.Infrastructure.Persistencia.Configuraciones;

public class TipoEstafaConfiguracion : IEntityTypeConfiguration<TipoEstafa>
{
    public void Configure(EntityTypeBuilder<TipoEstafa> builder)
    {
        builder.ToTable("TiposEstafa");
        builder.HasKey(t => t.Id);

        // Los Ids los define el catálogo (constantes en TipoEstafa), no la base de datos.
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Nombre).HasMaxLength(100).IsRequired();
        builder.HasIndex(t => t.Nombre).IsUnique();

        // Datos semilla: quedan dentro de la migración, así cualquier base nueva ya los tiene.
        builder.HasData(
            new TipoEstafa(TipoEstafa.FalsoBanco, "Falso banco"),
            new TipoEstafa(TipoEstafa.PaqueteRetenido, "Paquete retenido"),
            new TipoEstafa(TipoEstafa.FalsoFamiliar, "Falso familiar"),
            new TipoEstafa(TipoEstafa.PremioFalso, "Premio falso"),
            new TipoEstafa(TipoEstafa.FalsaOfertaTrabajo, "Falsa oferta de trabajo"),
            new TipoEstafa(TipoEstafa.InversionFalsa, "Inversión falsa"),
            new TipoEstafa(TipoEstafa.Otro, "Otro"),
            new TipoEstafa(TipoEstafa.Ninguno, "Ninguno"));
    }
}
