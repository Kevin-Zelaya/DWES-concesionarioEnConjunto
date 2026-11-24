using APIAlquilerVehiculos.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace APIAlquilerVehiculos.Test.Infraestructure;

public static class TestServices
{
    public static ServiceProvider Build(string connectionString)
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddRepositories();
        services.AddServices(null);

        return services.BuildServiceProvider();
    }
}