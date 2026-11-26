
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
                await reservaService.ProcesarDevolucionesTardiasAsync(ct);
                await reservaService.ProcesarVehiculosNoDisponiblesAsync(ct);
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