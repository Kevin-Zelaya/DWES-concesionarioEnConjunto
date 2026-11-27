
using Serilog;

public class ReservaBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReservaBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();

                var reservaService = scope.ServiceProvider
                    .GetRequiredService<IReservaService>();
                // Procesa reservas pendientes
                await reservaService.ProcesarReservasPendientesAsync(ct);
                // Procesar devoluciones tardias
                await reservaService.ProcesarDevolucionesTardiasAsync(ct);
                // Procesar vehiculos no disponibles
                await reservaService.ProcesarVehiculosNoDisponiblesAsync(ct);
                // Procesar reservas no pagadas
                await reservaService.ProcesarReservasPendientesAsync(ct);
            }
            catch (OperationCanceledException)
                when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error al procesar las reservas automáticamente.");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(1),
                ct);
        }
    }
}