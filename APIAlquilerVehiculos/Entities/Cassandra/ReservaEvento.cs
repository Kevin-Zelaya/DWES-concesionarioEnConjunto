
namespace APIAlquilerVehiculos.Entities.Cassandra;

/// <summary>
/// Representa un evento relacionado con una reserva.
/// Se utiliza para almacenar en Cassandra los acontecimientos
/// necesarios para calcular las estadísticas del negocio.
/// </summary>
public class ReservaEvento
{
    /// <summary>
    /// Identificador único del evento.
    /// </summary>
    public Guid EventoId { get; set; }

    /// <summary>
    /// Identificador de la reserva relacionada con el evento.
    /// </summary>
    public long ReservaId { get; set; }

    /// <summary>
    /// Código de referencia de la reserva.
    /// </summary>
    public string CodigoReferencia { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de evento: ReservaCreada, VehiculoRecogido
    /// o VehiculoDevuelto.
    /// </summary>
    public string TipoEvento { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora en la que se produjo el evento.
    /// </summary>
    public DateTime FechaEvento { get; set; }

    /// <summary>
    /// Identificador del vehículo relacionado con la reserva.
    /// </summary>
    public string VehiculoId { get; set; } = string.Empty;

    /// <summary>
    /// Modelo del vehículo reservado.
    /// </summary>
    public string Modelo { get; set; } = string.Empty;

    /// <summary>
    /// Categoría del vehículo reservado.
    /// </summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Lista de pólizas contratadas en la reserva.
    /// </summary>
    public List<string> Polizas { get; set; } = [];
    
    /// <summary>
    /// Fecha prevista de recogida del vehículo.
    /// </summary>
    public DateTime FechaRecogida { get; set; }

    /// <summary>
    /// Fecha prevista de devolución del vehículo.
    /// </summary>
    public DateTime FechaDevolucion { get; set; }
}
