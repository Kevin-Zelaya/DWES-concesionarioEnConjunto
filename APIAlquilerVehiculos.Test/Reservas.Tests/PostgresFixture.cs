using Testcontainers.PostgreSql;

namespace APIAlquilerVehiculos.Tests.Infrastructure;

public class PostgresFixture
{
    private readonly PostgreSqlContainer _container;

    public string ConnectionString => _container.GetConnectionString();

    public PostgresFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase("AlquilerVehiculosTests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async Task StartAsync()
    {
        await _container.StartAsync();
    }

    public async Task StopAsync()
    {
        await _container.DisposeAsync();
    }
}