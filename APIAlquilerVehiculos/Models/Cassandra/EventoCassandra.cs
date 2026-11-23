
using APIAlquilerVehiculos.Models.Cassandra.Enums;
namespace APIAlquilerVehiculos.Models.Cassandra;

/// <summary>
/// Representa un evento importante ocurrido en el negocio.
/// Estos eventos se almacenarán en Cassandra para generar estadísticas.
/// </summary>
public class EventoCassandra
{
    /// <summary>
    /// Identificador único del evento.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador de la reserva relacionada con el evento.
    /// </summary>
    public long ReservaId { get; set; }

    /// <summary>
    /// Identificador del vehículo relacionado con el evento.
    /// </summary>
    public string VehiculoId { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de evento ocurrido.
    /// </summary>
public TipoEvento Tipo { get; set; }
    /// <summary>
    /// Fecha y hora en la que ocurrió el evento.
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Importe relacionado con el evento, si corresponde.
    /// </summary>
    public decimal? Importe { get; set; }
}
