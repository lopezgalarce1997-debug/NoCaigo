namespace NoCaigo.Application.Interfaces;

/// <summary>
/// Reemplaza datos personales por marcadores antes de guardar el texto
/// o enviarlo a un proveedor de IA externo.
/// </summary>
public interface IAnonimizador
{
    string Anonimizar(string texto);
}
