using APIAlquilerVehiculos.Repositories.Clientes;
using APIAlquilerVehiculos.Repositories.Pagos;
using APIAlquilerVehiculos.Repositories.Trabajadores;
using APIAlquilerVehiculos.Repositories.Vehiculos;
using Serilog;

public static class RepositoryConfiguration
{
    public static IServiceCollection AddRepositories(
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
        services.AddScoped<ITrabajadorRepository, TrabajadorRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IPagoRepository, PagoRepository>();
        return services;
    }
}