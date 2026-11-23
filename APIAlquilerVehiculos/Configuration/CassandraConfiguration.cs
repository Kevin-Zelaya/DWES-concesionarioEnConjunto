
using APIAlquilerVehiculos.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace APIAlquilerVehiculos.Configuration;

/// <summary>
/// Clase encargada de configurar y registrar Cassandra
/// en el sistema de inyección de dependencias de la aplicación.
/// </summary>
public static class CassandraConfiguration
{
    /// <summary>
    /// Obtiene la configuración de Cassandra, establece una conexión
    /// y registra la sesión para que pueda utilizarse desde otras clases.
    /// </summary>
    /// <param name="services">
    /// Contenedor de servicios donde se registra la sesión de Cassandra.
    /// </param>
    /// <param name="configuration">
    /// Configuración de la aplicación, desde donde se obtienen
    /// los datos de conexión de Cassandra.
    /// </param>
    /// <returns>
    /// El contenedor de servicios con la sesión de Cassandra registrada.
    /// </returns>
    public static IServiceCollection AddCassandra(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Obtenemos la sección Cassandra del appsettings.json
        // y convertimos sus valores en un objeto CassandraConfig.
        var cassandraConfig = configuration
            .GetSection("Cassandra")
            .Get<CassandraConfig>();

        // Creamos la conexión utilizando los datos de configuración.
        // El operador ! indica que confiamos en que la configuración
        // no sea nula.
        var session = CassandraConnection.Connect(cassandraConfig!);

        // Registramos la sesión como Singleton para reutilizarla
        // durante toda la ejecución de la aplicación.
        services.AddSingleton<Cassandra.ISession>(session);

        // Devolvemos el contenedor de servicios actualizado.
        return services;
    }
}
