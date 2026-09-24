using NoCaigo.Application.Reglas;
using NoCaigo.Domain.Entidades;

namespace NoCaigo.Tests.Application.Reglas;

public class ReglaUrgenciaTests
{
    private readonly ReglaUrgencia _regla = new();

    [Theory]
    [InlineData("Tu cuenta será bloqueada hoy")]
    [InlineData("ÚLTIMO AVISO: regulariza tu deuda")]
    [InlineData("Debes responder inmediatamente")]
    [InlineData("Tienes 24 horas para confirmar")]
    [InlineData("Tu tarjeta fue suspendida")]
    public void Evaluar_TextoConUrgencia_SeActiva(string texto)
    {
        var resultado = _regla.Evaluar(texto);

        Assert.True(resultado.Activada);
        Assert.Equal(ReglaUrgencia.PuntosRiesgo, resultado.Puntos);
    }

    [Theory]
    [InlineData("Nos vemos el viernes en la oficina")]
    [InlineData("Cuidado con el hoyo en la calle")] // "hoy" dentro de otra palabra no cuenta
    public void Evaluar_TextoSinUrgencia_NoSeActiva(string texto)
    {
        var resultado = _regla.Evaluar(texto);

        Assert.False(resultado.Activada);
        Assert.Equal(0, resultado.Puntos);
    }

    [Fact]
    public void Evaluar_DescripcionMuestraLasPalabrasConSusTildesOriginales()
    {
        var resultado = _regla.Evaluar("Último aviso: tu cuenta está bloqueada");

        Assert.Contains("\"Último aviso\"", resultado.Descripcion);
        Assert.Contains("\"bloqueada\"", resultado.Descripcion);
    }
}

public class ReglaPedidoDatosSensiblesTests
{
    private readonly ReglaPedidoDatosSensibles _regla = new();

    [Theory]
    [InlineData("Ingresa tu clave para validar")]
    [InlineData("Envíanos tu tarjeta de coordenadas")]
    [InlineData("Dinos el código de verificación que te llegó")]
    [InlineData("Confirma los datos de tu tarjeta")]
    [InlineData("Necesitamos tu CVV")]
    [InlineData("Actualiza tu contraseña aquí")]
    public void Evaluar_PideDatosSensibles_SeActiva(string texto)
    {
        var resultado = _regla.Evaluar(texto);

        Assert.True(resultado.Activada);
        Assert.Equal(ReglaPedidoDatosSensibles.PuntosRiesgo, resultado.Puntos);
    }

    [Theory]
    [InlineData("Tu pedido fue despachado")]
    [InlineData("Código de seguimiento: 12345")] // un código de seguimiento no es secreto
    public void Evaluar_NoPideDatosSensibles_NoSeActiva(string texto)
    {
        Assert.False(_regla.Evaluar(texto).Activada);
    }
}

public class ReglaPedidoPagoTests
{
    private readonly ReglaPedidoPago _regla = new();

    [Theory]
    [InlineData("Transfiere $50.000 a esta cuenta")]
    [InlineData("Debes pagar el arancel de aduana")]
    [InlineData("Deposita en mi Cuenta RUT")]
    [InlineData("Haz la transferencia antes de las 18:00")]
    public void Evaluar_PidePago_SeActiva(string texto)
    {
        var resultado = _regla.Evaluar(texto);

        Assert.True(resultado.Activada);
        Assert.Equal(ReglaPedidoPago.PuntosRiesgo, resultado.Puntos);
    }

    [Theory]
    [InlineData("Hola mamá, llego tarde a cenar")]
    [InlineData("Tu pedido ya está en camino")]
    public void Evaluar_NoPidePago_NoSeActiva(string texto)
    {
        Assert.False(_regla.Evaluar(texto).Activada);
    }
}

public class ReglaPremioInesperadoTests
{
    private readonly ReglaPremioInesperado _regla = new();

    [Theory]
    [InlineData("¡Felicidades! Ganaste un iPhone")]
    [InlineData("Has sido seleccionado para el sorteo")]
    [InlineData("Reclama tu bono de invierno")]
    public void Evaluar_PrometePremio_SeActivaYSugierePremioFalso(string texto)
    {
        var resultado = _regla.Evaluar(texto);

        Assert.True(resultado.Activada);
        Assert.Equal(ReglaPremioInesperado.PuntosRiesgo, resultado.Puntos);
        Assert.Equal(TipoEstafa.PremioFalso, resultado.TipoEstafaSugeridoId);
    }

    [Theory]
    [InlineData("Mañana hay reunión de apoderados")]
    [InlineData("Ganamos el partido ayer")]
    public void Evaluar_SinPremio_NoSeActiva(string texto)
    {
        Assert.False(_regla.Evaluar(texto).Activada);
    }
}
