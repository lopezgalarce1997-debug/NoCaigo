using System.Text.Json.Serialization;
using NoCaigo.Api.Errores;
using NoCaigo.Application;
using NoCaigo.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Cada capa registra sus propios servicios; Program.cs solo las compone.
builder.Services.AgregarAplicacion();
builder.Services.AgregarInfraestructura(builder.Configuration);

builder.Services
    .AddControllers()
    // Enums como texto en el JSON ("Estafa" en vez de 3): más legible para quien consume la API.
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorErroresGlobal>();

// Documento OpenAPI nativo de .NET (/openapi/v1.json); Swagger UI solo lo muestra.
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/openapi/v1.json", "NoCaigo API v1");
        o.DocumentTitle = "NoCaigo API";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
