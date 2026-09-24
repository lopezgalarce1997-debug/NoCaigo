using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NoCaigo.Application.Interfaces;
using NoCaigo.Infrastructure.Persistencia;
using NoCaigo.Infrastructure.Persistencia.Repositorios;

namespace NoCaigo.Infrastructure;

/// <summary>
/// Punto único donde Infrastructure registra sus servicios. La Api solo llama a
/// AgregarInfraestructura y no necesita conocer EF Core ni los detalles internos.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AgregarInfraestructura(
        this IServiceCollection services, IConfiguration configuration)
    {
        var cadenaConexion = configuration.GetConnectionString("NoCaigo")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'NoCaigo'.");

        services.AddDbContext<NoCaigoDbContext>(opciones => opciones.UseSqlServer(cadenaConexion));
        services.AddScoped<IRepositorioAnalisis, RepositorioAnalisis>();

        return services;
    }
}
