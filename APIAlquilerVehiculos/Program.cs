using APIAlquilerVehiculos;
using APIAlquilerVehiculos.Seed;
using APIAlquilerVehiculos.Services;
using APIAlquilerVehiculos.Services.Auth;
using APIAlquilerVehiculos.Validators.Auth;
using FluentValidation;
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

Log.Information("Iniciando aplicación...");

// ======= Configuración de Swagger ==========
builder.Services.AddEndpointsApiExplorer();
builder.AddCustomSwagger();

// ======= Registrando controladores ==========
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true;
});

// ======= Infraestructura ==========
builder.Services
    .AddDatabases(builder.Configuration);
builder.Services.AddAuthenticationConfing(builder.Configuration);

builder.Services.AddScoped<IAuthService, AuthService>();




// ======= FluentValidation =======
builder.Services.AddValidatorsFromAssemblyContaining<SignupDtoValidator>();

// ======= MIDDLEWARES ==========
var app = builder.Build();

var apiConfig = app.Services
    .GetRequiredService<IOptions<ApiConfig>>()
    .Value;

// Seed de Usuarios
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider
        .GetRequiredService<ILogger<Program>>();

    await UserRoleSeeder.SeedIdentityAsync(
        scope.ServiceProvider,
        logger
    );
}

// Inicializar bases de datos
await app.InitializeDatabasesAsync();

if (apiConfig.shouldRecreateDb)
{
    // Inicialización del servicio storage
    app.InitializeStorage(true);

    // Inicialización de Mongo y PostgreSQL
    await app.RecreateDatabasesAsync();
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
Console.WriteLine($"  Swagger:          http://localhost:5205/swagger");
Console.WriteLine($"  Reconstruir Db:   {(apiConfig.shouldRecreateDb ? "Sí" : "No")}");

Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine("==============================================");
Console.ResetColor();
Console.WriteLine();
app.Run();
