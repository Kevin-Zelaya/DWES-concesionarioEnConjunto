
using Cassandra;

namespace APIAlquilerVehiculos.Configuration;

/// <summary>
/// Clase encargada de establecer una conexión con Cassandra
/// utilizando los datos proporcionados en CassandraConfig.
/// </summary>
public class CassandraConnection
{
    /// <summary>
    /// Crea una conexión con el clúster de Cassandra y selecciona
    /// el keyspace indicado en la configuración.
    /// </summary>
    /// <param name="config">
    /// Objeto que contiene la dirección, el puerto y el nombre
    /// del keyspace de Cassandra.
    /// </param>
    /// <returns>
    /// Una sesión de Cassandra que permite realizar consultas
    /// y operaciones sobre el keyspace seleccionado.
    /// </returns>
    public static Cassandra.ISession Connect(CassandraConfig config)
    {
        // Construimos el clúster indicando dónde se encuentra Cassandra
        // y el puerto utilizado para establecer la conexión.
        var cluster = Cluster.Builder()
            .AddContactPoint(config.ContactPoint)
            .WithPort(config.Port)
            .Build();

        // Establecemos la conexión seleccionando directamente
        // el keyspace indicado en la configuración.
        return cluster.Connect(config.Keyspace);
    }
}
