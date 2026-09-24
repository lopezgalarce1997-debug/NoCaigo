namespace NoCaigo.Application.Reglas;

/// <summary>Detecta pedidos de pago, depósito o transferencia de dinero.</summary>
public sealed class ReglaPedidoPago() : ReglaPorPalabrasClave(
[
    "transfier[ae]s?",
    "transferir",
    "transferencia",
    "deposit[ae]s?",
    "depositar",
    "deposito",
    "pag[ae]",
    "pagar",
    "pago",
    "abon[ae]",
    "abonar",
    "cuenta rut",
])
{
    public const int PuntosRiesgo = 20;

    protected override int Puntos => PuntosRiesgo;

    protected override string Describir(IReadOnlyList<string> encontradas)
        => $"Te pide dinero ({Citar(encontradas)}). Verifica por un canal oficial antes de pagar o transferir.";
}
