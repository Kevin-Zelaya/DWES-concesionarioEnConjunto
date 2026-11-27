using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace APIAlquilerVehiculos.Test.Infraestructure;

public static class TestServices
{
    public static ServiceProvider Build(string connectionString)
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EmailSettings:Server"] = "localhost",
                ["EmailSettings:Port"] = "1025",
                ["EmailSettings:SenderName"] = "Test Alquiler",
                ["EmailSettings:SenderEmail"] = "noreply@funkoapi.com",
                ["EmailSettings:UserName"] = "test",
                ["EmailSettings:Password"] = "test",
                ["EmailSettings:EnableSsl"] = "false"
            })
            .Build();

        services.AddLogging();

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddRepositories();
        services.AddServices(configuration);

        return services.BuildServiceProvider();
    }
}