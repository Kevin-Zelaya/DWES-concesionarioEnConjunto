
using Cassandra;
using APIAlquilerVehiculos.Configuration;
using Serilog;

/// <summary>
/// Clase encargada de configurar y registrar la conexión con Cassandra
/// en el contenedor de inyección de dependencias de la aplicación.
/// </summary>
public static class CassandraConnectionConfig
{
    /// <summary>
    /// Configura la conexión con Cassandra, crea el keyspace si no existe
    /// y registra el clúster y la sesión para poder utilizarlos
    /// desde otras clases de la aplicación.
    /// </summary>
    /// <param name="services">
    /// Contenedor de servicios donde se registran las dependencias.
    /// </param>
    /// <param name="configuration">
    /// Configuración de la aplicación, desde donde se obtienen
    /// los datos de conexión a Cassandra.
    /// </param>
    /// <returns>
    /// El mismo contenedor de servicios con la conexión a Cassandra registrada.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Se produce si no se encuentra la configuración de Cassandra.
    /// </exception>
    public static IServiceCollection AddCassandraConnection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Registramos en el log que comienza la configuración de Cassandra.
        Log.Information("Configurando la conexión a Cassandra...");

        // Obtenemos la sección Cassandra del archivo appsettings.json
        // y convertimos sus valores en un objeto CassandraConfig.
        var cassandraConfig = configuration
            .GetSection("Cassandra")
            .Get<CassandraConfig>();

        // Comprobamos que la configuración se haya encontrado correctamente.
        if (cassandraConfig == null)
        {
            throw new InvalidOperationException(
                "No se encontró la configuración de Cassandra.");
        }

        // Construimos el clúster indicando la dirección y el puerto
        // donde se encuentra el servidor Cassandra.
        var cluster = Cluster.Builder()
            .AddContactPoint(cassandraConfig.ContactPoint)
            .WithPort(cassandraConfig.Port)
            .Build();

        // Establecemos una conexión inicial sin seleccionar ningún keyspace.
        var session = cluster.Connect();

        // Creamos el keyspace configurado si todavía no existe.
        // SimpleStrategy con factor de replicación 1 se utiliza
        // para trabajar con un único nodo en el entorno de desarrollo.
        session.Execute($@"
            CREATE KEYSPACE IF NOT EXISTS {cassandraConfig.Keyspace}
            WITH replication = {{
                'class': 'SimpleStrategy',
                'replication_factor': 1
            }}");

        // Seleccionamos el keyspace en el que trabajará la aplicación.
        session.ChangeKeyspace(cassandraConfig.Keyspace);

        // Registramos el clúster como servicio Singleton para reutilizarlo
        // durante toda la ejecución de la aplicación.
        services.AddSingleton<ICluster>(cluster);

        // Registramos la sesión de Cassandra para poder inyectarla
        // en repositorios y servicios cuando sea necesario.
        services.AddSingleton<Cassandra.ISession>(session);

        // Registramos que la conexión se ha configurado correctamente.
        Log.Information("Conexión a Cassandra configurada correctamente.");

        // Devolvemos el contenedor de servicios con las dependencias registradas.
        return services;
    }
}
