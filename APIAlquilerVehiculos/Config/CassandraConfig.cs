
namespace APIAlquilerVehiculos.Configuration;
public class CassandraConfig
{
    public string ContactPoint { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 9042;
    public string Keyspace { get; set; } = "alquiler_vehiculos";
}
