using System.Text.RegularExpressions;
using NoCaigo.Application.Interfaces;
using NoCaigo.Domain.Entidades;

namespace NoCaigo.Application.Reglas;

/// <summary>
/// Detecta links cuyo dominio usa el nombre de un banco o empresa chilena conocida
/// pero no es su dominio oficial (por ejemplo "bancoestado-seguro.com").
/// </summary>
public sealed partial class ReglaDominioSuplantado : IReglaDeteccion
{
    public const int PuntosRiesgo = 35;

    private sealed record Marca(string Nombre, string[] PalabrasClave, string[] DominiosOficiales, int TipoEstafaId);

    private static readonly Marca[] Marcas =
    [
        new("BancoEstado", ["bancoestado", "banco-estado"], ["bancoestado.cl"], TipoEstafa.FalsoBanco),
        new("Banco de Chile", ["bancochile", "bancodechile", "banchile"], ["bancochile.cl", "banchile.cl"], TipoEstafa.FalsoBanco),
        new("Santander", ["santander"], ["santander.cl"], TipoEstafa.FalsoBanco),
        new("BCI", ["bci"], ["bci.cl"], TipoEstafa.FalsoBanco),
        new("Scotiabank", ["scotiabank"], ["scotiabankchile.cl", "scotiabank.cl"], TipoEstafa.FalsoBanco),
        new("Itaú", ["itau"], ["itau.cl"], TipoEstafa.FalsoBanco),
        new("Banco Falabella", ["falabella"], ["bancofalabella.cl", "falabella.com", "falabella.cl"], TipoEstafa.FalsoBanco),
        new("Correos de Chile", ["correos", "correoschile"], ["correos.cl"], TipoEstafa.PaqueteRetenido),
        new("Chilexpress", ["chilexpress"], ["chilexpress.cl"], TipoEstafa.PaqueteRetenido),
        new("Starken", ["starken"], ["starken.cl"], TipoEstafa.PaqueteRetenido),
        new("Blue Express", ["blueexpress", "blue-express"], ["blue.cl"], TipoEstafa.PaqueteRetenido),
        new("SII", ["sii"], ["sii.cl"], TipoEstafa.Otro),
        new("Mercado Libre", ["mercadolibre"], ["mercadolibre.cl", "mercadolibre.com"], TipoEstafa.Otro),
        new("ChileAtiende", ["chileatiende"], ["chileatiende.gob.cl"], TipoEstafa.Otro),
    ];

    public ResultadoRegla Evaluar(string texto)
    {
        ArgumentNullException.ThrowIfNull(texto);

        foreach (Match m in RegexDominio().Matches(texto.ToLowerInvariant()))
        {
            var dominio = m.Groups["dominio"].Value;

            foreach (var marca in Marcas)
            {
                if (UsaNombreDeMarca(dominio, marca) && !EsDominioOficial(dominio, marca))
                {
                    return new ResultadoRegla(true, PuntosRiesgo,
                        $"El link \"{dominio}\" parece de {marca.Nombre}, pero no es su sitio oficial " +
                        $"({marca.DominiosOficiales[0]}). Es una técnica típica para robar datos.",
                        marca.TipoEstafaId);
                }
            }
        }

        return ResultadoRegla.NoActivada;
    }

    private static bool UsaNombreDeMarca(string dominio, Marca marca)
        => marca.PalabrasClave.Any(clave => clave.Length <= 3
            // Palabras cortas ("bci", "sii") deben ser una parte completa del dominio,
            // para no calzar por casualidad dentro de otra palabra.
            ? dominio.Split('.', '-').Contains(clave)
            : dominio.Contains(clave));

    // Es oficial si es el dominio exacto o un subdominio suyo (www.bancoestado.cl).
    // "bancoestado.cl.verificar.com" NO lo es: el dominio real ahí es verificar.com.
    private static bool EsDominioOficial(string dominio, Marca marca)
        => marca.DominiosOficiales.Any(oficial => dominio == oficial || dominio.EndsWith("." + oficial));

    // Dominios con o sin http(s)://. El lookbehind evita tomar la parte después de una @.
    [GeneratedRegex(
        @"(?<![@\w.-])(?:https?://)?(?<dominio>(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z]{2,})(?![\w-])",
        RegexOptions.CultureInvariant, 200)]
    private static partial Regex RegexDominio();
}
