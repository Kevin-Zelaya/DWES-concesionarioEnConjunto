namespace APIAlquilerVehiculos.Services.Email;

public class EmailBackgroundWorker(
    IEmailQueue emailQueue,
    IServiceScopeFactory scopeFactory,
    ILogger<EmailBackgroundWorker> logger) : BackgroundService
{
    private const int MaxReintentos = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker de envío de correos iniciado.");

        await foreach (var mensaje in emailQueue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var resultado = await emailService.SendEmailAsync(
                    mensaje.ToEmail,
                    mensaje.Subject,
                    mensaje.BodyHtml);

                if (resultado.IsFailure)
                {
                    await GestionarReintentoAsync(mensaje, resultado.Error.Message, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                await GestionarReintentoAsync(mensaje, ex.Message, stoppingToken);
            }
        }
    }

    private async Task GestionarReintentoAsync(EmailQueueMessage mensaje, string motivoError, CancellationToken ct)
    {
        var siguienteIntento = mensaje.Intentos + 1;

        if (siguienteIntento <= MaxReintentos)
        {
            logger.LogWarning(
                "Fallo enviando correo a {ToEmail} (intento {Intento}/{Max}). Motivo: {Error}. Reencolando...",
                mensaje.ToEmail, siguienteIntento, MaxReintentos, motivoError);

            await Task.Delay(TimeSpan.FromSeconds(5 * siguienteIntento), ct);
            await emailQueue.EnqueueRetryAsync(mensaje with { Intentos = siguienteIntento }, ct);
        }
        else
        {
            logger.LogError(
                "Correo descartado definitivamente tras {Max} intentos para {ToEmail} | Asunto: '{Subject}' | Último error: {Error}",
                MaxReintentos, mensaje.ToEmail, mensaje.Subject, motivoError);
        }
    }
}