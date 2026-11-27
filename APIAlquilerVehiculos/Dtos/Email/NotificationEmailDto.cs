namespace APIAlquilerVehiculos.Dtos.Email;

public enum TipoAvisoEmail
{
    Exito,       // Verde (#28a745)
    Info,        // Azul/Morado (#667eea)
    Advertencia, // Amarillo (#ffc107)
    Peligro      // Rojo (#dc3545)
}

/// <summary>
/// Contrato común que implementan todos los DTOs de notificación por correo.
/// </summary>
public interface IEmailNotificacionDto
{
    string EmailDestino { get; }
    string NombreCliente { get; }
    string Asunto { get; }
}

/// <summary>
/// DTO genérico clave-valor por si alguien quiere enviar un aviso rápido sin crear un DTO nuevo.
/// </summary>
public record NotificacionEmailDto(
    string EmailDestino,
    string NombreCliente,
    string Asunto,
    string TituloCabecera,
    Dictionary<string, string> CamposTabla,
    string MensajeDestacado,
    TipoAvisoEmail TipoAviso = TipoAvisoEmail.Info
) : IEmailNotificacionDto;

// ============================================================================
// DTOs ESPECÍFICOS POR MÓDULO (Cada miembro del equipo añade aquí los suyos)
// ============================================================================

/// <summary>
/// [Módulo Taller - RF-07 / RF-34] Aviso al cliente cuando su vehículo reservado entra en taller.
/// </summary>
public record ParteTallerEmailDto(
    string EmailDestino,
    string NombreCliente,
    string CodigoReferencia,
    string VehiculoOriginal,
    string DescripcionAveria,
    string ResolucionAplicada,
    string DetalleResolucion
) : IEmailNotificacionDto
{
    public string Asunto => $"Aviso sobre tu reserva {CodigoReferencia} - Revisión en taller";
}

// TODO (Compañero Reservas): Añadir aquí AltaReservaEmailDto, CancelacionReservaEmailDto, RecogidaEmailDto, DevolucionEmailDto
// TODO (Compañero Pagos): Añadir aquí PagoAprobadoEmailDto, ReembolsoEmailDto
// TODO (Compañero Tareas): Añadir aquí CaducidadReservaEmailDto