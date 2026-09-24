using NoCaigo.Application.Reglas;
using NoCaigo.Domain.Entidades;

namespace NoCaigo.Tests.Application.Reglas;

public class ReglaLinkAcortadoTests
{
    private readonly ReglaLinkAcortado _regla = new();

    [Theory]
    [InlineData("Revisa tu paquete aquí: bit.ly/3xYz12")]
    [InlineData("Ingresa a https://tinyurl.com/abc123")]
    [InlineData("Link: CUTT.LY/promo")]
    public void Evaluar_ConLinkAcortado_SeActiva(string texto)
    {
        var resultado = _regla.Evaluar(texto);

        Assert.True(resultado.Activada);
        Assert.Equal(ReglaLinkAcortado.PuntosRiesgo, resultado.Puntos);
    }

    [Theory]
    [InlineData("Ingresa a https://www.bancoestado.cl")]
    [InlineData("Escríbenos por chat.com")] // "t.co" dentro de otra palabra no cuenta
    [InlineData("Sin links en este mensaje")]
    public void Evaluar_SinLinkAcortado_NoSeActiva(string texto)
    {
        Assert.False(_regla.Evaluar(texto).Activada);
    }
}

public class ReglaDominioSuplantadoTests
{
    private readonly ReglaDominioSuplantado _regla = new();

    [Theory]
    [InlineData("Valida tu cuenta en https://bancoestado-seguro.com", TipoEstafa.FalsoBanco)]
    [InlineData("Ingresa a www.santander-cl.net/login", TipoEstafa.FalsoBanco)]
    [InlineData("Entra a bancoestado.cl.verificar.com", TipoEstafa.FalsoBanco)]
    [InlineData("Tu paquete está retenido: correoschile-envios.com", TipoEstafa.PaqueteRetenido)]
    [InlineData("Paga en http://chilexpress.pagos.xyz", TipoEstafa.PaqueteRetenido)]
    [InlineData("Devolución de impuestos en sii-devolucion.com", TipoEstafa.Otro)]
    public void Evaluar_DominioQueImitaMarca_SeActivaConTipoSugerido(string texto, int tipoEsperado)
    {
        var resultado = _regla.Evaluar(texto);

        Assert.True(resultado.Activada);
        Assert.Equal(ReglaDominioSuplantado.PuntosRiesgo, resultado.Puntos);
        Assert.Equal(tipoEsperado, resultado.TipoEstafaSugeridoId);
    }

    [Theory]
    [InlineData("Ingresa a https://www.bancoestado.cl")]
    [InlineData("Revisa tu envío en correos.cl/seguimiento")]
    [InlineData("Consulta en https://homer.sii.cl")]
    [InlineData("Visita abcinfo.com")] // "bci" dentro de otra palabra no cuenta
    [InlineData("Tu cuenta Santander tiene un aviso")] // menciona la marca, pero sin link
    public void Evaluar_DominioOficialOSinLink_NoSeActiva(string texto)
    {
        Assert.False(_regla.Evaluar(texto).Activada);
    }

    [Fact]
    public void Evaluar_Activada_DescripcionIndicaElDominioOficial()
    {
        var resultado = _regla.Evaluar("https://bancoestado-seguro.com");

        Assert.Contains("bancoestado-seguro.com", resultado.Descripcion);
        Assert.Contains("bancoestado.cl", resultado.Descripcion);
    }
}
