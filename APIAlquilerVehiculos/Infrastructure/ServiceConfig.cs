using APIAlquilerVehiculos.Configuration;
using APIAlquilerVehiculos.Services;
using APIAlquilerVehiculos.Services.Auth;
using APIAlquilerVehiculos.Services.Clientes;
using APIAlquilerVehiculos.Services.Email;
using APIAlquilerVehiculos.Services.Pagos;
using APIAlquilerVehiculos.Services.Trabajadores;
using Serilog;
using APIAlquilerVehiculos.Services.Pagos;
using APIAlquilerVehiculos.Services.Taller;

public static class ServiceConfig
{
    public static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        Log.Information("Configurando servicios...");
        // Reservas
        services.AddScoped<IReservaService, ReservaService>();
        services.AddScoped<ILineaReservaService, LineaReservaService>();
        services.AddScoped<ILineaReservaFactory, LineaReservaFactory>();
        services.AddScoped<ICache, RedisCache>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<ITrabajadorService, TrabajadorService>();
        services.AddScoped<IDisponibilidadService, DisponibilidadService>();
        services.AddScoped<IParteService, ParteService>();
        services.AddScoped<IPasarelaPago, PasarelaPagoSimulada>();
        services.AddScoped<IPagoService, PagoService>();
        services.AddScoped<ICifradorTarjeta, CifradorTarjeta>();
        services.AddSingleton<IncidenciaService>();
        services.AddScoped<IExtraService, ExtraService>();
        services.AddScoped<IPolizaService, PolizaService>();
        // Email
        services.AddOptions<EmailSettings>()
            .Bind(configuration.GetSection(EmailSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IEmailQueue, EmailQueue>();
        services.AddHostedService<EmailBackgroundWorker>();
        services.AddScoped<IEmailService, EmailService>();
        //services.AddScoped<INotificationEmailService, NotificationEmailService>();
        return services;
    }
}