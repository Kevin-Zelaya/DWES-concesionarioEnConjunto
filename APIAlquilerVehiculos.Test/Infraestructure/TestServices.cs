using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Services;
using APIAlquilerVehiculos.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace APIAlquilerVehiculos.Tests.Infrastructure;

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
        services.AddServices();

        return services.BuildServiceProvider();
    }
}