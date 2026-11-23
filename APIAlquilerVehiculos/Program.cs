using System.Text.Json.Serialization;
using APIAlquilerVehiculos.Infrastructure.AutenticationJWTAutorization;
using APIAlquilerVehiculos.Seed;
using APIAlquilerVehiculos.Services;
using APIAlquilerVehiculos.Services.Auth;
using APIAlquilerVehiculos.Validators.Auth;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ======= Logger de Serilog =============
Console.OutputEncoding = System.Text.Encoding.UTF8;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/productos-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
// Integrar Serilog con ASP.NET Core
builder.Host.UseSerilog();
Log.Information("Iniciando aplicación...");

// ======= Configuración de Swagger ==========
builder.Services.AddEndpointsApiExplorer();
builder.AddCustomSwagger();

// ======= Registrando controladores ==========
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true;
}).AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });;

// ======= Infraestructura ==========
builder.Services
    .AddDatabases(builder.Configuration)
    .AddServices()
    .AddRepositories()
    .AddAuthenticationConfing(builder.Configuration)
    .AddRedisConnection(builder.Configuration);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

// ======= FluentValidation =======
builder.Services.AddValidatorsFromAssemblyContaining<SignupDtoValidator>();

// ======= MIDDLEWARES ==========

var app = builder.Build();
app.UseGlobalExceptionHandler();
var apiConfig = app.Services
    .GetRequiredService<IOptions<ApiConfig>>()
    .Value;

// Inicializar bases de datos
await app.InitializeDatabasesAsync();

if (apiConfig.shouldRecreateDb)
{
    await app.RecreateDatabasesAsync();
    app.InitializeStorage(true);
}
else
{
    await app.InitializeDatabasesAsync();
}
// Seed de Usuarios
// esto se debe controlar de otra forma por el modo desarrollo y producción
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider
        .GetRequiredService<ILogger<Program>>();

    await UserRoleSeeder.SeedIdentityAsync(
        scope.ServiceProvider,
        logger
    );
    await CatalogoSeeder.SeedMarcasAsync(
        scope.ServiceProvider,
        logger
    );
    await CatalogoSeeder.SeedPolizasAsync(
        scope.ServiceProvider,
        logger
    );
    await CatalogoSeeder.SeedVehiculosAsync(
        scope.ServiceProvider,
        logger,
        1
    );
}
//app.UseHttpsRedirection();

// Permite lectura de archivos estáticos
app.UseStaticFiles();

// ======= AUTHENTICATION - AUTHORIZATION ==========
app.UseAuthentication();  // Primero: quien eres?
app.UseAuthorization();   // Segundo: que puedes hacer?

// Documentacón solo en Desarrollo
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Alquiler Vehículos v1"));
}
// ======= Buscar todos los controladores ==========
app.MapControllers();


// TODO: Falta vaciar la BD en el caso de Mongo
Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine("==============================================");
Console.WriteLine("                Concesionario                  ");
Console.WriteLine("==============================================");
Console.ResetColor();


Console.WriteLine($"  Entorno:          {app.Environment.EnvironmentName}");
Console.WriteLine($"  Swagger:          http://localhost:5017/swagger");
Console.WriteLine($"  Reconstruir Db:   {(apiConfig.shouldRecreateDb ? "Sí" : "No")}");

Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine("==============================================");
Console.ResetColor();
Console.WriteLine();
app.Run();
