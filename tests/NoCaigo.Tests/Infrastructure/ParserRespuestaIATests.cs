using NoCaigo.Application.Interfaces;
using NoCaigo.Domain.Entidades;
using NoCaigo.Domain.Enums;
using NoCaigo.Infrastructure.IA;

namespace NoCaigo.Tests.Infrastructure;

public class ParserRespuestaIATests
{
    private const string JsonValido = """
        {
          "tipoEstafa": "Falso banco",
          "nivelRiesgo": 90,
          "veredicto": "Estafa",
          "senales": ["Pide tu clave", "Link que imita al banco"],
          "explicacion": "No entregues tu clave."
        }
        """;

    [Fact]
    public void Parsear_JsonValido_DevuelveTodosLosCampos()
    {
        var r = ParserRespuestaIA.Parsear(JsonValido);

        Assert.Equal(TipoEstafa.FalsoBanco, r.TipoEstafaId);
        Assert.Equal(90, r.NivelRiesgo);
        Assert.Equal(Veredicto.Estafa, r.Veredicto);
        Assert.Equal(["Pide tu clave", "Link que imita al banco"], r.Senales);
        Assert.Equal("No entregues tu clave.", r.Explicacion);
    }

    [Fact]
    public void Parsear_JsonEnvueltoEnMarkdown_LoExtrae()
    {
        var r = ParserRespuestaIA.Parsear($"Aquí está el análisis:\n```json\n{JsonValido}\n```");

        Assert.Equal(90, r.NivelRiesgo);
    }

    [Theory]
    [InlineData("falso BANCO")]
    [InlineData("Inversion falsa")] // sin tilde
    public void Parsear_TipoSinImportarMayusculasNiTildes_LoReconoce(string tipo)
    {
        var r = ParserRespuestaIA.Parsear($$"""{"tipoEstafa": "{{tipo}}", "nivelRiesgo": 70}""");

        Assert.NotEqual(TipoEstafa.Otro, r.TipoEstafaId);
    }

    [Fact]
    public void Parsear_TipoInventado_UsaOtro()
    {
        var r = ParserRespuestaIA.Parsear("""{"tipoEstafa": "Phishing avanzado", "nivelRiesgo": 70}""");

        Assert.Equal(TipoEstafa.Otro, r.TipoEstafaId);
    }

    [Theory]
    [InlineData("150", 100)]
    [InlineData("-10", 0)]
    [InlineData("\"85\"", 85)] // número entre comillas
    [InlineData("72.6", 73)]
    public void Parsear_NivelRiesgo_SeNormalizaAlRango(string valorJson, int esperado)
    {
        var r = ParserRespuestaIA.Parsear($$"""{"nivelRiesgo": {{valorJson}}}""");

        Assert.Equal(esperado, r.NivelRiesgo);
    }

    [Theory]
    [InlineData("\"Tal vez\"")]
    [InlineData("\"3\"")] // Enum.TryParse aceptaría "3"; no debe
    [InlineData("null")]
    public void Parsear_VeredictoInvalido_SeCalculaDesdeElRiesgo(string veredictoJson)
    {
        var r = ParserRespuestaIA.Parsear($$"""{"nivelRiesgo": 30, "veredicto": {{veredictoJson}}}""");

        Assert.Equal(Veredicto.Sospechoso, r.Veredicto);
    }

    [Fact]
    public void Parsear_Senales_DescartaVaciasYNoTextoYLimitaCantidad()
    {
        var r = ParserRespuestaIA.Parsear("""
            {"nivelRiesgo": 50, "senales": ["a", "", 3, "b", "c", "d", "e", "f", "g"]}
            """);

        Assert.Equal(["a", "b", "c", "d", "e"], r.Senales);
    }

    [Fact]
    public void Parsear_SinCamposOpcionales_UsaValoresPorDefecto()
    {
        var r = ParserRespuestaIA.Parsear("""{"nivelRiesgo": 10}""");

        Assert.Equal(TipoEstafa.Otro, r.TipoEstafaId);
        Assert.Equal(Veredicto.Seguro, r.Veredicto);
        Assert.Empty(r.Senales);
        Assert.Equal(string.Empty, r.Explicacion);
    }

    [Fact]
    public void Parsear_ExplicacionMuyLarga_SeRecorta()
    {
        var larga = new string('x', 5000);

        var r = ParserRespuestaIA.Parsear($$"""{"nivelRiesgo": 10, "explicacion": "{{larga}}"}""");

        Assert.Equal(ParserRespuestaIA.LargoMaximoExplicacion, r.Explicacion.Length);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("No puedo analizar este mensaje.")]  // sin JSON
    [InlineData("{\"nivelRiesgo\": 80,")]              // JSON cortado
    [InlineData("{\"tipoEstafa\": \"Falso banco\"}")]  // falta nivelRiesgo
    [InlineData("{\"nivelRiesgo\": \"alto\"}")]        // nivelRiesgo no numérico
    [InlineData("[1, 2, 3]")]                          // no es objeto
    public void Parsear_RespuestaMalFormada_LanzaExcepcionServicioIA(string? contenido)
    {
        Assert.Throws<ExcepcionServicioIA>(() => ParserRespuestaIA.Parsear(contenido));
    }
}
