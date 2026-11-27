using APIAlquilerVehiculos.Dtos.Email;

namespace APIAlquilerVehiculos.Services.Email;

public interface INotificationEmailService
{
    Task EnviarAsync(IEmailNotificacionDto dto, CancellationToken ct = default);
}