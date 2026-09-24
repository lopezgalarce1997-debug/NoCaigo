using Moq;
using NoCaigo.Application.Interfaces;
using NoCaigo.Application.Servicios;
using NoCaigo.Domain.Entidades;

namespace NoCaigo.Tests.Application;

public class EvaluadorReglasTests
{
    // Reglas falsas con Moq: se prueba la combinación sin depender de las reglas reales.
    private static IReglaDeteccion Regla(ResultadoRegla resultado)
    {
        var mock = new Mock<IReglaDeteccion>();
        mock.Setup(r => r.Evaluar(It.IsAny<string>())).Returns(resultado);
        return mock.Object;
    }

    private static ResultadoRegla Activada(int puntos, int? tipo = null)
        => new(true, puntos, $"Señal de {puntos} puntos", tipo);

    [Fact]
    public void Evaluar_SinReglasActivadas_RiesgoCeroYTipoNinguno()
    {
        var evaluador = new EvaluadorReglas([Regla(ResultadoRegla.NoActivada), Regla(ResultadoRegla.NoActivada)]);

        var resultado = evaluador.Evaluar("texto");

        Assert.Equal(0, resultado.NivelRiesgo);
        Assert.Equal(TipoEstafa.Ninguno, resultado.TipoEstafaId);
        Assert.Empty(resultado.Activadas);
    }

    [Fact]
    public void Evaluar_SumaLosPuntosDeLasReglasActivadas()
    {
        var evaluador = new EvaluadorReglas([Regla(Activada(20)), Regla(Activada(15)), Regla(ResultadoRegla.NoActivada)]);

        var resultado = evaluador.Evaluar("texto");

        Assert.Equal(35, resultado.NivelRiesgo);
        Assert.Equal(2, resultado.Activadas.Count);
    }

    [Fact]
    public void Evaluar_SumaMayorA100_SeLimitaA100()
    {
        var evaluador = new EvaluadorReglas([Regla(Activada(60)), Regla(Activada(60))]);

        Assert.Equal(100, evaluador.Evaluar("texto").NivelRiesgo);
    }

    [Fact]
    public void Evaluar_UsaElTipoSugeridoPorLaReglaDeMasPuntos()
    {
        var evaluador = new EvaluadorReglas(
        [
            Regla(Activada(25, TipoEstafa.PremioFalso)),
            Regla(Activada(35, TipoEstafa.FalsoBanco)),
            Regla(Activada(40)) // más puntos, pero no sugiere tipo
        ]);

        Assert.Equal(TipoEstafa.FalsoBanco, evaluador.Evaluar("texto").TipoEstafaId);
    }

    [Fact]
    public void Evaluar_ActivadasSinTipoSugerido_TipoOtro()
    {
        var evaluador = new EvaluadorReglas([Regla(Activada(15))]);

        Assert.Equal(TipoEstafa.Otro, evaluador.Evaluar("texto").TipoEstafaId);
    }
}
