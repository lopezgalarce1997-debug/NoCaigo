using Microsoft.Extensions.Options;
using NoCaigo.Application.Interfaces;
using NoCaigo.Application.Servicios;
using NoCaigo.Domain.Entidades;
using NoCaigo.Domain.Enums;

namespace NoCaigo.Tests.Application;

public class CombinadorPuntajesTests
{
    private static CombinadorPuntajes Combinador(double pesoReglas = 0.4, double pesoIA = 0.6)
        => new(Options.Create(new OpcionesCombinacion { PesoReglas = pesoReglas, PesoIA = pesoIA }));

    private static ResultadoReglas Reglas(int riesgo, int tipo = TipoEstafa.Otro) => new(riesgo, tipo, []);

    private static RespuestaIA IA(int riesgo, int tipo = TipoEstafa.Otro)
        => new(tipo, riesgo, Veredicto.Sospechoso, [], "explicación");

    // Casos concretos de la fórmula final = max(reglas, 0.4·reglas + 0.6·IA)
    [Theory]
    [InlineData(50, 90, 74)]  // 0.4·50 + 0.6·90 = 20 + 54 = 74 → la IA sube el riesgo
    [InlineData(0, 70, 42)]   // 0 + 42 = 42 → la IA detecta lo que las reglas no vieron
    [InlineData(60, 0, 60)]   // 24 + 0 = 24 < 60 → las reglas son el piso (IA manipulada)
    [InlineData(35, 30, 35)]  // 14 + 18 = 32 < 35 → la IA no puede bajar el riesgo
    [InlineData(15, 50, 36)]  // 6 + 30 = 36
    [InlineData(25, 40, 34)]  // 10 + 24 = 34
    [InlineData(100, 100, 100)]
    [InlineData(0, 0, 0)]
    public void Combinar_ConIA_AplicaPromedioPonderadoConPisoDeReglas(int reglas, int ia, int esperado)
    {
        var resultado = Combinador().Combinar(Reglas(reglas), IA(ia));

        Assert.Equal(esperado, resultado.NivelRiesgo);
        Assert.True(resultado.UsoIA);
    }

    [Fact]
    public void Combinar_RedondeaElPuntoMedioHaciaArriba()
    {
        // 0.5·20 + 0.5·55 = 37.5 → 38 (Math.Round por defecto daría 38 igual, pero con 36.5
        // daría 36 por "redondeo bancario"; AwayFromZero evita esa sorpresa).
        var resultado = Combinador(0.5, 0.5).Combinar(Reglas(20), IA(55));

        Assert.Equal(38, resultado.NivelRiesgo);
    }

    [Theory]
    [InlineData(0.5, 0.5, 20, 80, 50)]  // pesos iguales
    [InlineData(0.0, 1.0, 10, 70, 70)]  // solo IA (pero con piso de reglas)
    [InlineData(1.0, 0.0, 30, 90, 30)]  // solo reglas: la IA no influye
    public void Combinar_UsaLosPesosConfigurados(double pesoReglas, double pesoIA, int reglas, int ia, int esperado)
    {
        var resultado = Combinador(pesoReglas, pesoIA).Combinar(Reglas(reglas), IA(ia));

        Assert.Equal(esperado, resultado.NivelRiesgo);
    }

    [Fact]
    public void Combinar_SinIA_UsaSoloReglasEIndicaQueNoUsoIA()
    {
        var resultado = Combinador().Combinar(Reglas(45, TipoEstafa.FalsoBanco), ia: null);

        Assert.Equal(45, resultado.NivelRiesgo);
        Assert.Equal(Veredicto.Sospechoso, resultado.Veredicto);
        Assert.Equal(TipoEstafa.FalsoBanco, resultado.TipoEstafaId);
        Assert.False(resultado.UsoIA);
    }

    [Fact]
    public void Combinar_VeredictoSaleDelPuntajeFinalYNoDelDeLaIA()
    {
        // La IA dice "Sospechoso" pero el puntaje final (74) corresponde a Estafa.
        var resultado = Combinador().Combinar(Reglas(50), IA(90));

        Assert.Equal(Veredicto.Estafa, resultado.Veredicto);
    }

    [Fact]
    public void Combinar_TipoConcretoDeReglas_TienePrioridadSobreIA()
    {
        var resultado = Combinador().Combinar(Reglas(35, TipoEstafa.FalsoBanco), IA(80, TipoEstafa.PremioFalso));

        Assert.Equal(TipoEstafa.FalsoBanco, resultado.TipoEstafaId);
    }

    [Fact]
    public void Combinar_ReglasSinTipoConcreto_UsaElTipoDeLaIA()
    {
        // Caso "Hola mamá, número nuevo": las reglas solo ven un pedido de pago.
        var resultado = Combinador().Combinar(Reglas(20, TipoEstafa.Otro), IA(85, TipoEstafa.FalsoFamiliar));

        Assert.Equal(TipoEstafa.FalsoFamiliar, resultado.TipoEstafaId);
    }

    [Fact]
    public void Combinar_NadieDaTipoConcreto_UsaOtro()
    {
        var resultado = Combinador().Combinar(Reglas(40, TipoEstafa.Otro), IA(60, TipoEstafa.Ninguno));

        Assert.Equal(TipoEstafa.Otro, resultado.TipoEstafaId);
    }

    [Fact]
    public void Combinar_VeredictoSeguro_TipoNinguno()
    {
        var resultado = Combinador().Combinar(Reglas(15, TipoEstafa.Otro), IA(10, TipoEstafa.PremioFalso));

        Assert.Equal(Veredicto.Seguro, resultado.Veredicto);
        Assert.Equal(TipoEstafa.Ninguno, resultado.TipoEstafaId);
    }
}

public class OpcionesCombinacionTests
{
    [Theory]
    [InlineData(0.4, 0.6, true)]
    [InlineData(0.1, 0.9, true)]
    [InlineData(0.7, 0.3, true)]  // 0.7 + 0.3 no da exactamente 1.0 en double: se tolera
    [InlineData(0.0, 1.0, true)]
    [InlineData(0.5, 0.6, false)] // suman 1.1
    [InlineData(-0.2, 1.2, false)]
    public void EsValida_PesosEntre0y1QueSuman1(double pesoReglas, double pesoIA, bool esperado)
    {
        var opciones = new OpcionesCombinacion { PesoReglas = pesoReglas, PesoIA = pesoIA };

        Assert.Equal(esperado, opciones.EsValida());
    }
}
