using NoCaigo.Domain.Entidades;
using NoCaigo.Domain.Enums;

namespace NoCaigo.Tests.Domain;

public class AnalisisTests
{
    private static Analisis CrearAnalisis(
        string texto = "Mensaje de prueba",
        int nivelRiesgo = 50,
        DateTime? fecha = null)
        => new(texto, Canal.Sms, nivelRiesgo, Veredicto.Sospechoso, TipoEstafa.Otro,
               "Explicación", usoIA: false, fecha ?? DateTime.UtcNow);

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Constructor_NivelRiesgoEnLimites_SeCrea(int nivelRiesgo)
    {
        var analisis = CrearAnalisis(nivelRiesgo: nivelRiesgo);

        Assert.Equal(nivelRiesgo, analisis.NivelRiesgo);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Constructor_NivelRiesgoFueraDeRango_LanzaExcepcion(int nivelRiesgo)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CrearAnalisis(nivelRiesgo: nivelRiesgo));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_TextoVacio_LanzaExcepcion(string texto)
    {
        Assert.Throws<ArgumentException>(() => CrearAnalisis(texto: texto));
    }

    [Fact]
    public void Constructor_FechaNoUtc_LanzaExcepcion()
    {
        var fechaLocal = new DateTime(2026, 9, 24, 10, 0, 0, DateTimeKind.Local);

        Assert.Throws<ArgumentException>(() => CrearAnalisis(fecha: fechaLocal));
    }

    [Fact]
    public void AgregarSenal_AgregaLaSenalConSuOrigen()
    {
        var analisis = CrearAnalisis();

        analisis.AgregarSenal("Contiene un link acortado", OrigenSenal.Regla);

        var senal = Assert.Single(analisis.Senales);
        Assert.Equal("Contiene un link acortado", senal.Descripcion);
        Assert.Equal(OrigenSenal.Regla, senal.Origen);
    }
}
