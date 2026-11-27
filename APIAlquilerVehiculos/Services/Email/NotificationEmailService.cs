using APIAlquilerVehiculos.Dtos.Email;

namespace APIAlquilerVehiculos.Services.Email;

public class NotificationEmailService(
    IEmailQueue emailQueue,
    ILogger<NotificationEmailService> logger) : INotificationEmailService
{
    public async Task EnviarAsync(IEmailNotificacionDto dto, CancellationToken ct = default)
    {
        try
        {
            var html = EmailTemplates.GenerarHtml(dto);
            await emailQueue.EnqueueAsync(dto.EmailDestino, dto.Asunto, html, ct);
        }
        catch (Exception ex)
        {
            // RF-35: El envío de correo nunca bloquea ni rompe la operación de negocio
            logger.LogError(ex, "Error al encolar notificación {TipoDto} para {Email}",
                dto.GetType().Name, dto.EmailDestino);
        }
    }
}