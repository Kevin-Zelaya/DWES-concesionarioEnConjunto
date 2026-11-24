
using APIAlquilerVehiculos.Entities.Cassandra;
using Cassandra;

namespace APIAlquilerVehiculos.Services.Cassandra;

/// <summary>
/// Servicio encargado de almacenar en Cassandra
/// los eventos relacionados con las reservas.
/// </summary>
public class ReservaEventoService
{
    private readonly global::Cassandra.ISession _session;
    private readonly PreparedStatement _insertStatement;

    public ReservaEventoService(global::Cassandra.ISession session)  
      {
        _session = session;

        _insertStatement = _session.Prepare(@"
            INSERT INTO reserva_eventos (
                reserva_id,
                fecha_evento,
                evento_id,
                codigo_referencia,
                tipo_evento,
                vehiculo_id,
                modelo,
                categoria,
                polizas,
                fecha_recogida,
                fecha_devolucion
            )
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        ");
    }

    /// <summary>
    /// Guarda un evento de reserva en Cassandra.
    /// </summary>
    public async Task RegistrarEventoAsync(ReservaEvento evento)
    {
        var statement = _insertStatement.Bind(
            evento.ReservaId,
            evento.FechaEvento,
            evento.EventoId,
            evento.CodigoReferencia,
            evento.TipoEvento,
            evento.VehiculoId,
            evento.Modelo,
            evento.Categoria,
            evento.Polizas,
            evento.FechaRecogida,
            evento.FechaDevolucion
        );

        await _session.ExecuteAsync(statement);
    }
}
