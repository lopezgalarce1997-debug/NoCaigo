using NoCaigo.Domain.Enums;
using NoCaigo.Domain.Reglas;

namespace NoCaigo.Tests.Domain;

public class ClasificadorRiesgoTests
{
    [Theory]
    [InlineData(0, Veredicto.Seguro)]
    [InlineData(24, Veredicto.Seguro)]
    [InlineData(25, Veredicto.Sospechoso)]
    [InlineData(59, Veredicto.Sospechoso)]
    [InlineData(60, Veredicto.Estafa)]
    [InlineData(100, Veredicto.Estafa)]
    public void ObtenerVeredicto_SegunUmbrales(int nivelRiesgo, Veredicto esperado)
    {
        Assert.Equal(esperado, ClasificadorRiesgo.ObtenerVeredicto(nivelRiesgo));
    }
}
