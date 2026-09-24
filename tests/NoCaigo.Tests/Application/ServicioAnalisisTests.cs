using Moq;
using NoCaigo.Application.Dtos;
using NoCaigo.Application.Interfaces;
using NoCaigo.Application.Reglas;
using NoCaigo.Application.Servicios;
using NoCaigo.Domain.Entidades;
using NoCaigo.Domain.Enums;

namespace NoCaigo.Tests.Application;

public class ServicioAnalisisTests
{
    private readonly Mock<IRepositorioAnalisis> _repositorio = new();
    private Analisis? _guardado;

    private ServicioAnalisis CrearServicio()
    {
        // Captura lo que el servicio intenta guardar, para revisarlo en el Assert.
        _repositorio
            .Setup(r => r.AgregarAsync(It.IsAny<Analisis>(), It.IsAny<CancellationToken>()))
            .Callback<Analisis, CancellationToken>((a, _) => _guardado = a)
            .Returns(Task.CompletedTask);

        // Reglas y anonimizador reales: son lógica pura, no hace falta simularlos.
        var evaluador = new EvaluadorReglas(
            [new ReglaUrgencia(), new ReglaPedidoDatosSensibles(), new ReglaDominioSuplantado()]);

        return new ServicioAnalisis(new Anonimizador(), evaluador, _repositorio.Object, TimeProvider.System);
    }

    [Fact]
    public async Task AnalizarAsync_GuardaElTextoAnonimizado()
    {
        var servicio = CrearServicio();

        await servicio.AnalizarAsync(new SolicitudAnalisis
        {
            Texto = "Llama al +56 9 1234 5678, RUT 12.345.678-9",
            Canal = Canal.Sms
        });

        Assert.NotNull(_guardado);
        Assert.Equal("Llama al [TELEFONO], RUT [RUT]", _guardado.TextoAnonimizado);
    }

    [Fact]
    public async Task AnalizarAsync_MensajeFraudulento_VeredictoEstafaConSenalesDeReglas()
    {
        var servicio = CrearServicio();

        var resultado = await servicio.AnalizarAsync(new SolicitudAnalisis
        {
            Texto = "Tu cuenta fue bloqueada. Valida tu clave hoy en https://bancoestado-seguro.com",
            Canal = Canal.Sms
        });

        // Urgencia (15) + datos sensibles (35) + dominio suplantado (35) = 85
        Assert.Equal(85, resultado.NivelRiesgo);
        Assert.Equal(Veredicto.Estafa, resultado.Veredicto);
        Assert.Equal(3, resultado.Senales.Count);
        Assert.All(resultado.Senales, s => Assert.Equal(OrigenSenal.Regla, s.Origen));
        Assert.False(resultado.UsoIA);
        Assert.Equal(TipoEstafa.FalsoBanco, _guardado!.TipoEstafaId);
    }

    [Fact]
    public async Task AnalizarAsync_MensajeNormal_VeredictoSeguroSinSenales()
    {
        var servicio = CrearServicio();

        var resultado = await servicio.AnalizarAsync(new SolicitudAnalisis
        {
            Texto = "¿Nos juntamos el sábado a almorzar?",
            Canal = Canal.WhatsApp
        });

        Assert.Equal(0, resultado.NivelRiesgo);
        Assert.Equal(Veredicto.Seguro, resultado.Veredicto);
        Assert.Empty(resultado.Senales);
        Assert.Equal(TipoEstafa.Ninguno, _guardado!.TipoEstafaId);
    }

    [Fact]
    public async Task AnalizarAsync_SenalesNoContienenDatosPersonales()
    {
        var servicio = CrearServicio();

        var resultado = await servicio.AnalizarAsync(new SolicitudAnalisis
        {
            Texto = "Envía tu clave a juan@mail.cl hoy",
            Canal = Canal.Correo
        });

        Assert.DoesNotContain(resultado.Senales, s => s.Descripcion.Contains("juan@mail.cl"));
    }

    [Fact]
    public async Task ObtenerPorIdAsync_NoExiste_DevuelveNull()
    {
        _repositorio.Setup(r => r.ObtenerPorIdAsync(99, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Analisis?)null);

        var resultado = await CrearServicio().ObtenerPorIdAsync(99);

        Assert.Null(resultado);
    }
}
