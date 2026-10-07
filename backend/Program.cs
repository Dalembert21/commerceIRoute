using IRouteComercioApi.Datos;
using IRouteComercioApi.Servicios;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Inyección de Dependencias
builder.Services.AddScoped<IComercioRepositorio, ComercioRepositorio>();
builder.Services.AddScoped<IComercioServicio, ComercioServicio>();

// Configuración de Controladores
builder.Services.AddControllers()
    .AddJsonOptions(opciones =>
    {
        opciones.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Configuración de CORS para permitir conexión con Angular
builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy("PermitirFrontendAngular", politica =>
    {
        politica.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
    });
});

// Configuración de Documentación Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Entorno de Desarrollo y Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Comercios v1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz http://localhost:5225/
    });
}

app.UseCors("PermitirFrontendAngular");

app.UseAuthorization();

app.MapControllers();

app.Run();
