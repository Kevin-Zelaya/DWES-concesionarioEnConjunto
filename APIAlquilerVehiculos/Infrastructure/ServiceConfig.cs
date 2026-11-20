using APIAlquilerVehiculos.Services.Trabajadores;
using Serilog;

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
        services.AddScoped<ITrabajadorService, TrabajadorService>();
        return services;
    }
}