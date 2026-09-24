using NoCaigo.Application.Servicios;

namespace NoCaigo.Tests.Application;

public class AnonimizadorTests
{
    private readonly Anonimizador _anonimizador = new();

    [Theory]
    [InlineData("Llámame al +56 9 1234 5678")]
    [InlineData("Llámame al +56912345678")]
    [InlineData("Llámame al 56912345678")]
    [InlineData("Llámame al 912345678")]
    [InlineData("Llámame al 9 1234 5678")]
    [InlineData("Llámame al 9-1234-5678")]
    [InlineData("Llámame al +56 2 2345 6789")]
    public void Anonimizar_Telefono_LoReemplaza(string texto)
    {
        Assert.Equal("Llámame al [TELEFONO]", _anonimizador.Anonimizar(texto));
    }

    [Theory]
    [InlineData("Mi RUT es 12.345.678-9")]
    [InlineData("Mi RUT es 12345678-9")]
    [InlineData("Mi RUT es 9.876.543-K")]
    [InlineData("Mi RUT es 9876543-k")]
    public void Anonimizar_Rut_LoReemplaza(string texto)
    {
        Assert.Equal("Mi RUT es [RUT]", _anonimizador.Anonimizar(texto));
    }

    [Theory]
    [InlineData("Escribe a juan.perez@gmail.com")]
    [InlineData("Escribe a soporte+cuenta@banco-falso.cl")]
    public void Anonimizar_Correo_LoReemplaza(string texto)
    {
        Assert.Equal("Escribe a [CORREO]", _anonimizador.Anonimizar(texto));
    }

    [Theory]
    [InlineData("Tarjeta 4111111111111111 bloqueada")]
    [InlineData("Tarjeta 4111 1111 1111 1111 bloqueada")]
    [InlineData("Tarjeta 4111-1111-1111-1111 bloqueada")]
    public void Anonimizar_Tarjeta_LaReemplazaSinConfundirlaConTelefono(string texto)
    {
        Assert.Equal("Tarjeta [TARJETA] bloqueada", _anonimizador.Anonimizar(texto));
    }

    [Fact]
    public void Anonimizar_VariosDatos_ReemplazaTodos()
    {
        var texto = "Soy Pedro, RUT 12.345.678-9, fono +56 9 8765 4321, correo pedro@mail.cl";

        var resultado = _anonimizador.Anonimizar(texto);

        Assert.Equal("Soy Pedro, RUT [RUT], fono [TELEFONO], correo [CORREO]", resultado);
    }

    [Theory]
    [InlineData("Su pedido llegará mañana, gracias por su compra")]
    [InlineData("Tienes 3 días para pagar $15.000")]
    [InlineData("Código de seguimiento 12345")]
    [InlineData("Visita https://www.bancoestado.cl")]
    public void Anonimizar_SinDatosPersonales_NoCambiaElTexto(string texto)
    {
        Assert.Equal(texto, _anonimizador.Anonimizar(texto));
    }

    [Fact]
    public void Anonimizar_TextoNulo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentNullException>(() => _anonimizador.Anonimizar(null!));
    }
}
