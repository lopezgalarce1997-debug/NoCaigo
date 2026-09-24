using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NoCaigo.Application.Interfaces;
using NoCaigo.Application.Reglas;
using NoCaigo.Application.Servicios;

namespace NoCaigo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AgregarAplicacion(this IServiceCollection services, IConfiguration configuration)
    {
        // Sin estado y thread-safe: una sola instancia para toda la aplicación.
        services.AddSingleton<IAnonimizador, Anonimizador>();
        services.AddSingleton(TimeProvider.System);

        // Registro explícito de cada regla (en vez de escanear el ensamblado por reflexión):
        // se ve de un vistazo qué reglas están activas. Todas se inyectan como IEnumerable.
        services.AddSingleton<IReglaDeteccion, ReglaLinkAcortado>();
        services.AddSingleton<IReglaDeteccion, ReglaDominioSuplantado>();
        services.AddSingleton<IReglaDeteccion, ReglaUrgencia>();
        services.AddSingleton<IReglaDeteccion, ReglaPedidoDatosSensibles>();
        services.AddSingleton<IReglaDeteccion, ReglaPedidoPago>();
        services.AddSingleton<IReglaDeteccion, ReglaPremioInesperado>();
        services.AddSingleton<IReglaDeteccion, ReglaIntentoManipulacion>();
        services.AddSingleton<EvaluadorReglas>();

        // Pesos de la combinación reglas + IA desde appsettings. Si son inválidos, la API no arranca.
        services.AddOptions<OpcionesCombinacion>()
            .Bind(configuration.GetSection(OpcionesCombinacion.Seccion))
            .Validate(o => o.EsValida(), "Combinacion: PesoReglas y PesoIA deben estar entre 0 y 1 y sumar 1.")
            .ValidateOnStart();
        services.AddSingleton<CombinadorPuntajes>();

        // Scoped porque depende del repositorio, que usa el DbContext (uno por request).
        services.AddScoped<IServicioAnalisis, ServicioAnalisis>();

        return services;
    }
}
