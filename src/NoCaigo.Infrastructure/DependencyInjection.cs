using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NoCaigo.Application.Interfaces;
using NoCaigo.Infrastructure.IA;
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

        AgregarIA(services, configuration);

        return services;
    }

    private static void AgregarIA(IServiceCollection services, IConfiguration configuration)
    {
        // ValidateOnStart: si la configuración está mal (proveedor inexistente, URL inválida),
        // la API no arranca y lo dice claramente, en vez de fallar en el primer análisis.
        services.AddOptions<OpcionesIA>()
            .Bind(configuration.GetSection(OpcionesIA.Seccion))
            .Validate(o => o.ObtenerProveedorActivo() is not null,
                "IA:ProveedorActivo no coincide con ningún proveedor de IA:Proveedores.")
            .Validate(o => Uri.TryCreate(o.ObtenerProveedorActivo()?.UrlBase, UriKind.Absolute, out _),
                "La UrlBase del proveedor de IA activo no es una URL válida.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.ObtenerProveedorActivo()?.Modelo),
                "Falta el Modelo del proveedor de IA activo.")
            .Validate(o => o.TimeoutSegundos is > 0 and <= 120,
                "IA:TimeoutSegundos debe estar entre 1 y 120.")
            .ValidateOnStart();

        // Cliente tipado: IHttpClientFactory administra las conexiones y entrega el
        // HttpClient ya configurado con los datos del proveedor activo.
        services.AddHttpClient<IServicioIA, ClienteIACompatibleOpenAI>((sp, http) =>
        {
            var opciones = sp.GetRequiredService<IOptions<OpcionesIA>>().Value;
            var proveedor = opciones.ObtenerProveedorActivo()!;

            // La barra final importa: sin ella, "chat/completions" reemplazaría el "/v1" de la URL.
            http.BaseAddress = new Uri(proveedor.UrlBase.TrimEnd('/') + "/");
            http.Timeout = TimeSpan.FromSeconds(opciones.TimeoutSegundos);

            if (!string.IsNullOrWhiteSpace(proveedor.ApiKey))
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", proveedor.ApiKey);
        });
    }
}
