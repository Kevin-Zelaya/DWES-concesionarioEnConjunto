using Cassandra;
using APIAlquilerVehiculos.Configuration;
using Serilog;

public static class CassandraConnectionConfig
{
    public static IServiceCollection AddCassandraConnection(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        Log.Information("Configurando la conexión a Cassandra...");

        var cassandraConfig = configuration
            .GetSection("Cassandra")
            .Get<CassandraConfig>();

        var cluster = Cluster.Builder()
            .AddContactPoint(cassandraConfig!.ContactPoint)
            .WithPort(cassandraConfig.Port)
            .Build();

        // Nos conectamos primero sin seleccionar ningún keyspace
        var session = cluster.Connect();

        // Creamos el keyspace si todavía no existe
        session.Execute($@"
            CREATE KEYSPACE IF NOT EXISTS {cassandraConfig.Keyspace}
            WITH replication = {{
                'class': 'SimpleStrategy',
                'replication_factor': 1
            }};");

        // Seleccionamos el keyspace que vamos a utilizar
        session.ChangeKeyspace(cassandraConfig.Keyspace);

        services.AddSingleton<ICluster>(cluster);
        services.AddSingleton<Cassandra.ISession>(session);

        Log.Information("Conexión a Cassandra configurada correctamente.");

        return services;
    }
}