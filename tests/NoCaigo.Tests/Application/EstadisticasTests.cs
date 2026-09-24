using System.ComponentModel.DataAnnotations;
using Moq;
using NoCaigo.Application.Dtos;
using NoCaigo.Application.Interfaces;
using NoCaigo.Application.Servicios;

namespace NoCaigo.Tests.Application;

public class SemanaIsoTests
{
    [Theory]
    [InlineData("2026-09-21", "2026-09-21")] // lunes → el mismo día
    [InlineData("2026-09-23", "2026-09-21")] // miércoles
    [InlineData("2026-09-27", "2026-09-21")] // domingo → pertenece a la semana que empezó el lunes anterior
    [InlineData("2026-09-28", "2026-09-28")] // lunes siguiente → semana nueva
    public void Inicio_DevuelveElLunesDeLaSemana(string fecha, string lunesEsperado)
    {
        Assert.Equal(DateOnly.Parse(lunesEsperado), SemanaIso.Inicio(DateOnly.Parse(fecha)));
    }

    [Theory]
    [InlineData("2026-09-23", "2026-W39")]
    [InlineData("2026-01-01", "2026-W01")] // jueves 1 de enero → semana 1 del mismo año
    [InlineData("2026-12-31", "2026-W53")] // 2026 tiene 53 semanas ISO
    [InlineData("2027-01-01", "2026-W53")] // viernes 1 de enero de 2027 → ¡semana 53 de 2026!
    [InlineData("2027-01-03", "2026-W53")] // domingo
    [InlineData("2027-01-04", "2027-W01")] // primer lunes → semana 1 de 2027
    [InlineData("2024-12-30", "2025-W01")] // lunes 30 de diciembre de 2024 → semana 1 de 2025
    public void Etiqueta_UsaElAnioYSemanaIso(string fecha, string esperado)
    {
        Assert.Equal(esperado, SemanaIso.Etiqueta(DateOnly.Parse(fecha)));
    }
}

public class ServicioEstadisticasTests
{
    /// <summary>Reloj fijo: las pruebas no dependen del día en que se ejecutan.</summary>
    private sealed class RelojFijo(DateTimeOffset ahora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => ahora;
    }

    private readonly Mock<IRepositorioEstadisticas> _repositorio = new();
    private DateTime _desdeConsultado;
    private DateTime _hastaConsultado;

    private ServicioEstadisticas Servicio(string hoyUtc)
        => new(_repositorio.Object, new RelojFijo(DateTimeOffset.Parse(hoyUtc + "T15:00:00Z")));

    private void ConteosPorDia(params (string Dia, int Cantidad)[] conteos)
        => _repositorio
            .Setup(r => r.ContarPorDiaAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Callback<DateTime, DateTime, CancellationToken>((d, h, _) => (_desdeConsultado, _hastaConsultado) = (d, h))
            .ReturnsAsync(conteos.Select(c => new ConteoPorDia(DateOnly.Parse(c.Dia), c.Cantidad)).ToList());

    [Fact]
    public async Task ObtenerTendenciaAsync_AgrupaPorSemanaIsoEIncluyeSemanasVacias()
    {
        // Hoy: miércoles 23-09-2026 (semana 39). 3 semanas → W37, W38 y W39.
        ConteosPorDia(
            ("2026-09-07", 2),  // lunes W37
            ("2026-09-13", 1),  // domingo W37 (no W38: la semana empieza el lunes)
            ("2026-09-21", 1),  // lunes W39
            ("2026-09-27", 4)); // domingo W39

        var tendencia = await Servicio("2026-09-23").ObtenerTendenciaAsync(3);

        Assert.Equal(
        [
            new TendenciaSemanalDto("2026-W37", new DateOnly(2026, 9, 7), new DateOnly(2026, 9, 13), 3),
            new TendenciaSemanalDto("2026-W38", new DateOnly(2026, 9, 14), new DateOnly(2026, 9, 20), 0),
            new TendenciaSemanalDto("2026-W39", new DateOnly(2026, 9, 21), new DateOnly(2026, 9, 27), 5),
        ], tendencia);
    }

    [Fact]
    public async Task ObtenerTendenciaAsync_ConsultaDesdeElLunesInicialHastaElLunesSiguienteEnUtc()
    {
        ConteosPorDia();

        await Servicio("2026-09-23").ObtenerTendenciaAsync(3);

        Assert.Equal(new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc), _desdeConsultado);
        Assert.Equal(new DateTime(2026, 9, 28, 0, 0, 0, DateTimeKind.Utc), _hastaConsultado);
        Assert.Equal(DateTimeKind.Utc, _desdeConsultado.Kind);
    }

    [Fact]
    public async Task ObtenerTendenciaAsync_CruceDeAnio_UsaSemanasIsoCorrectas()
    {
        // Hoy: viernes 1-1-2027, que pertenece a 2026-W53.
        ConteosPorDia(("2026-12-31", 2), ("2027-01-01", 3));

        var tendencia = await Servicio("2027-01-01").ObtenerTendenciaAsync(2);

        Assert.Equal(["2026-W52", "2026-W53"], tendencia.Select(t => t.Semana));
        Assert.Equal(5, tendencia[1].Cantidad);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(53)]
    public async Task ObtenerTendenciaAsync_SemanasFueraDeRango_LanzaExcepcion(int semanas)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Servicio("2026-09-23").ObtenerTendenciaAsync(semanas));
    }

    [Fact]
    public async Task ObtenerPorTipoAsync_CalculaPorcentajesYOrdenaDeMayorAMenor()
    {
        _repositorio
            .Setup(r => r.ContarPorTipoAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new(1, "Falso banco", 1), new(2, "Paquete retenido", 0), new(4, "Premio falso", 2)]);

        var estadisticas = await Servicio("2026-09-23").ObtenerPorTipoAsync(new FiltroFechas());

        Assert.Equal(
        [
            new EstadisticaPorTipoDto(4, "Premio falso", 2, 66.7),
            new EstadisticaPorTipoDto(1, "Falso banco", 1, 33.3),
            new EstadisticaPorTipoDto(2, "Paquete retenido", 0, 0),
        ], estadisticas);
    }

    [Fact]
    public async Task ObtenerPorTipoAsync_SinAnalisis_PorcentajesEnCeroSinDividirPorCero()
    {
        _repositorio
            .Setup(r => r.ContarPorTipoAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new(1, "Falso banco", 0), new(8, "Ninguno", 0)]);

        var estadisticas = await Servicio("2026-09-23").ObtenerPorTipoAsync(new FiltroFechas());

        Assert.All(estadisticas, e => Assert.Equal(0, e.Porcentaje));
    }

    [Fact]
    public async Task ObtenerPorTipoAsync_HastaIncluyeElDiaCompleto()
    {
        DateTime? desde = null, hasta = null;
        _repositorio
            .Setup(r => r.ContarPorTipoAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Callback<DateTime?, DateTime?, CancellationToken>((d, h, _) => (desde, hasta) = (d, h))
            .ReturnsAsync([]);

        await Servicio("2026-09-23").ObtenerPorTipoAsync(
            new FiltroFechas { Desde = new DateOnly(2026, 9, 1), Hasta = new DateOnly(2026, 9, 30) });

        Assert.Equal(new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc), desde);
        Assert.Equal(new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc), hasta);
    }
}

public class FiltroFechasTests
{
    [Theory]
    [InlineData("2026-09-01", "2026-09-30", true)]
    [InlineData("2026-09-15", "2026-09-15", true)] // un solo día
    [InlineData(null, "2026-09-30", true)]
    [InlineData("2026-09-30", "2026-09-01", false)] // desde posterior a hasta
    public void Validate_DesdeNoPuedeSerPosteriorAHasta(string? desde, string? hasta, bool esValido)
    {
        var filtro = new FiltroFechas
        {
            Desde = desde is null ? null : DateOnly.Parse(desde),
            Hasta = hasta is null ? null : DateOnly.Parse(hasta)
        };

        var valido = Validator.TryValidateObject(filtro, new ValidationContext(filtro), [], validateAllProperties: true);

        Assert.Equal(esValido, valido);
    }
}
