using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NoCaigo.Domain.Entidades;

namespace NoCaigo.Infrastructure.Persistencia.Configuraciones;

public class AnalisisConfiguracion : IEntityTypeConfiguration<Analisis>
{
    // El texto de entrada se limita a menos en la API; aquí se deja margen porque
    // los marcadores del anonimizador ([TELEFONO], [TARJETA]...) pueden alargar el texto.
    public const int LargoMaximoTexto = 4000;
    public const int LargoMaximoExplicacion = 2000;

    public void Configure(EntityTypeBuilder<Analisis> builder)
    {
        builder.ToTable("Analisis");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.TextoAnonimizado).HasMaxLength(LargoMaximoTexto).IsRequired();
        builder.Property(a => a.Explicacion).HasMaxLength(LargoMaximoExplicacion).IsRequired();

        // Enums guardados como texto: la base es legible al consultarla directamente
        // y no se rompe si alguien reordena los valores del enum.
        builder.Property(a => a.Canal).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Veredicto).HasConversion<string>().HasMaxLength(20);

        // datetime2 no guarda la zona horaria: al leer, EF devuelve DateTimeKind.Unspecified
        // y el JSON sale sin la "Z". Como siempre se guarda en UTC, se marca como UTC al leer.
        builder.Property(a => a.FechaCreacion)
            .HasColumnType("datetime2")
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        builder.HasOne(a => a.TipoEstafa)
            .WithMany()
            .HasForeignKey(a => a.TipoEstafaId)
            .OnDelete(DeleteBehavior.Restrict); // No se puede borrar un tipo que ya se usó.

        builder.HasMany(a => a.Senales)
            .WithOne()
            .HasForeignKey(s => s.AnalisisId)
            .OnDelete(DeleteBehavior.Cascade); // Las señales no existen sin su análisis.

        // EF escribe directamente en el campo privado _senales (la propiedad es de solo lectura).
        builder.Navigation(a => a.Senales).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Índices para los filtros del listado y las estadísticas (por fecha y por tipo).
        builder.HasIndex(a => a.FechaCreacion);
        builder.HasIndex(a => a.TipoEstafaId);
    }
}
