using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SkillAlexa.API.Middleware;
using SkillAlexa.BC.Interfaces;
using SkillAlexa.BW.DTOs;
using SkillAlexa.BW.Services;
using SkillAlexa.BW.Validators;
using SkillAlexa.DA.Context;
using SkillAlexa.DA.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configuración de la base de datos
builder.Services.AddDbContext<SkillAlexaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro de repositorios
builder.Services.AddScoped<IListaCompraRepository, ListaCompraRepository>();
builder.Services.AddScoped<IItemListaRepository, ItemListaRepository>();

// Registro de servicios
builder.Services.AddScoped<IListaCompraService, ListaCompraService>();
builder.Services.AddScoped<IProductoService, ProductoService>();

// Registro de validadores
builder.Services.AddScoped<IValidator<CrearListaCompraDto>, CrearListaCompraValidator>();
builder.Services.AddScoped<IValidator<AgregarProductoDto>, AgregarProductoValidator>();

// Configuración de controladores
builder.Services.AddControllers();

// Configuración de Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Alexa Shopping List API",
        Version = "v1",
        Description = "API para gestión de listas de compras con integración Alexa",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Desarrollo",
            Email = "dev@example.com"
        }
    });

    // Incluir comentarios XML si existen
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configuración del pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Alexa Shopping List API v1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz
    });
}

// Middleware personalizado de manejo de errores
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
