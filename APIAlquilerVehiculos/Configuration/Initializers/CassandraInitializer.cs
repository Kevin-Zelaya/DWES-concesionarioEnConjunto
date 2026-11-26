
using Cassandra;

namespace APIAlquilerVehiculos.Configuration.Initializers;

/// <summary>
/// Se encarga de crear las tablas necesarias en Cassandra
/// para almacenar los eventos de las reservas.
/// </summary>
public class CassandraInitializer
{
    private readonly Cassandra.ISession _session;

    public CassandraInitializer(Cassandra.ISession session)
    {
        _session = session;
    }

    /// <summary>
    /// Crea la tabla donde se almacenarán los eventos de las reservas.
    /// </summary>
    public void Initialize()
    {
        _session.Execute(@"
            CREATE TABLE IF NOT EXISTS reserva_eventos (
                reserva_id bigint,
                fecha_evento timestamp,
                evento_id uuid,
                codigo_referencia text,
                tipo_evento text,
                vehiculo_id text,
                modelo text,
                categoria text,
                polizas list<text>,
                fecha_recogida timestamp,
                fecha_devolucion timestamp,
                PRIMARY KEY ((reserva_id), fecha_evento, evento_id)
            ) WITH CLUSTERING ORDER BY (fecha_evento ASC)
        ");
    }
}
