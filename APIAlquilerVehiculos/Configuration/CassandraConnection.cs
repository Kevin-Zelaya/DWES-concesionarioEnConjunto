
using Cassandra;

namespace APIAlquilerVehiculos.Configuration;
public class CassandraConnection
{
    public static Cassandra.ISession Connect(CassandraConfig config)
    {
        var cluster = Cluster.Builder()
            .AddContactPoint(config.ContactPoint)
            .WithPort(config.Port)
            .Build();

        return cluster.Connect(config.Keyspace);
    }
}
