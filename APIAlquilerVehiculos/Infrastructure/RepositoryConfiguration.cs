using APIAlquilerVehiculos.Repositories.Vehiculos;
using Serilog;

public static class RepositoryConfiguration
{
    public static IServiceCollection AddServices(
        this IServiceCollection services
    )
    {
        Log.Information("Configurando servicios...");
        // Reservas
        services.AddScoped<IReservaRepository, ReservaRepository>();
        services.AddScoped<ILineaReservaRepository, LineaReservaRepository>();
        services.AddScoped<IExtraRepository, ExtraRepository>();
        services.AddScoped<IRecargoRepository, RecargoRepository>();
        services.AddScoped<IPolizaRepository, PolizaRepository>();
        services.AddScoped<IVehiculosRepository, VehiculoRepository>();
        return services;
    }
}