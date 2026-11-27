using Serilog;

public class ReservaIncidenciaBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReservaIncidenciaBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            Log.Information("[BACKGROUND-INCIDENCIAS] Iniciando comprobación de incidencias pendientes de notificar.");

            try
            {
                using var scope = scopeFactory.CreateScope();

                var incidenciaService = scope.ServiceProvider
                    .GetRequiredService<IncidenciaService>();

                var notificationService = scope.ServiceProvider
                    .GetRequiredService<NotificationService>();

                // Obtener todas las incidencias registradas.
                var incidencias = incidenciaService.GetAll();

                foreach (var incidencia in incidencias)
                {
                    ct.ThrowIfCancellationRequested();

                    var toNotificate = incidencia.Value;

                    // Comprobar si ya se ha notificado en las últimas 24 horas.
                    if (toNotificate.FechaUltimaNotificacion.HasValue &&
                        toNotificate.FechaUltimaNotificacion.Value > DateTime.UtcNow.AddHours(-24))
                        continue;

                    // Se notifica por SignalR.
                    await notificationService.NotifyUserAsync(
                        toNotificate.ClienteId.ToString(),
                        new ResourceNotification<string>
                        {
                            Resource = "Reserva",
                            Action = "Incidencia",
                            ResourceId = toNotificate.CodigoReferencia,
                            Message = toNotificate.Descripcion,
                            Data = new
                            {
                                toNotificate.CodigoReferencia
                            },
                            CreatedAt = toNotificate.FechaDeteccion
                        });

                    // Actualizar la fecha de la última notificación.
                    toNotificate = toNotificate with
                    {
                        FechaUltimaNotificacion = DateTime.UtcNow
                    };

                    // Actualizar la incidencia en memoria.
                    incidenciaService.Update(incidencia.Key, toNotificate);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al procesar las incidencias automáticamente.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), ct);
        }
    }
}
