using APIAlquilerVehiculos.Services;
using APIAlquilerVehiculos.Services.Auth;
using APIAlquilerVehiculos.Services.Clientes;
using APIAlquilerVehiculos.Services.Pagos;
using APIAlquilerVehiculos.Services.Taller;
using APIAlquilerVehiculos.Services.Trabajadores;
using Serilog;
using APIAlquilerVehiculos.Services.Cassandra;
using APIAlquilerVehiculos.Services.Marcas;
using APIAlquilerVehiculos.Services.Pagos;
using APIAlquilerVehiculos.Services.Usuarios;

public static class ServiceConfig
{
    public static IServiceCollection AddServices(
        this IServiceCollection services
    )
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
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton<IReservaEventoService, ReservaEventoService>();
        services.AddScoped<IMarcaService, MarcaService>();
        return services;
    }
}