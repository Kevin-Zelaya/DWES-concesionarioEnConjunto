
using APIAlquilerVehiculos.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace APIAlquilerVehiculos.Configuration;

public static class CassandraConfiguration
{
    public static IServiceCollection AddCassandra(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Obtenemos la configuración de Cassandra
        var cassandraConfig = configuration
            .GetSection("Cassandra")
            .Get<CassandraConfig>();

        // Creamos la conexión y la registramos para poder utilizarla
        var session = CassandraConnection.Connect(cassandraConfig!);

        services.AddSingleton<Cassandra.ISession>(session);

        return services;
    }
}
