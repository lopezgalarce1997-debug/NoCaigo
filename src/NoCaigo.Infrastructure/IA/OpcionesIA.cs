namespace NoCaigo.Infrastructure.IA;

/// <summary>
/// Sección "IA" de la configuración. Cambiar de proveedor es cambiar ProveedorActivo:
/// todos los proveedores exponen la misma API compatible con OpenAI.
/// </summary>
public sealed class OpcionesIA
{
    public const string Seccion = "IA";

    /// <summary>Clave dentro de <see cref="Proveedores"/> que se usa (por ejemplo "Ollama" o "Groq").</summary>
    public string ProveedorActivo { get; set; } = string.Empty;

    /// <summary>Tiempo máximo de espera. Si se supera, el análisis sigue solo con reglas.</summary>
    public int TimeoutSegundos { get; set; } = 30;

    /// <summary>
    /// Tiempo máximo solo para ESTABLECER la conexión (distinto de esperar la respuesta).
    /// Si el proveedor está caído se detecta rápido y el análisis sigue con reglas,
    /// en vez de esperar los reintentos de conexión del sistema operativo.
    /// </summary>
    public double TimeoutConexionSegundos { get; set; } = 1;

    public Dictionary<string, OpcionesProveedorIA> Proveedores { get; set; } = [];

    /// <summary>Datos del proveedor activo (sin distinguir mayúsculas), o null si no está configurado.</summary>
    public OpcionesProveedorIA? ObtenerProveedorActivo()
        => Proveedores
            .FirstOrDefault(p => string.Equals(p.Key, ProveedorActivo, StringComparison.OrdinalIgnoreCase))
            .Value;
}

public sealed class OpcionesProveedorIA
{
    /// <summary>URL base de la API compatible con OpenAI (termina en /v1).</summary>
    public string UrlBase { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    /// <summary>
    /// Clave de la API. Ollama no la necesita. Para Groq NUNCA va en appsettings.json:
    /// se configura con User Secrets en local y con variables de entorno en un servidor.
    /// </summary>
    public string? ApiKey { get; set; }
}
