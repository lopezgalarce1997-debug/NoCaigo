using System.Text.Json.Serialization;
using NoCaigo.Api.Errores;
using NoCaigo.Application;
using NoCaigo.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Cada capa registra sus propios servicios; Program.cs solo las compone.
builder.Services.AgregarAplicacion();
builder.Services.AgregarInfraestructura(builder.Configuration);

// Enums como texto en el JSON ("Estafa" en vez de 3): más legible para quien consume la API.
// Se configura dos veces porque los controladores usan sus propias opciones de JSON (AddJsonOptions)
// y el generador de OpenAPI lee las opciones generales (ConfigureHttpJsonOptions); si solo se
// configurara una, Swagger documentaría los enums como números aunque la API responda texto.
builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorErroresGlobal>();

// Documento OpenAPI nativo de .NET (/openapi/v1.json); Swagger UI solo lo muestra.
builder.Services.AddOpenApi(o => o.AddDocumentTransformer((documento, _, _) =>
{
    documento.Info.Title = "NoCaigo API";
    documento.Info.Version = "v1";
    documento.Info.Description =
        "Detector de estafas en mensajes (SMS, WhatsApp, correo). " +
        "El resultado es orientativo y no reemplaza verificar con la entidad oficial.";
    return Task.CompletedTask;
}));

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
