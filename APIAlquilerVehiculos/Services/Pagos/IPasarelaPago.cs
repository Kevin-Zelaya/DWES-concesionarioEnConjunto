using APIAlquilerVehiculos.Models.Pagos;

namespace APIAlquilerVehiculos.Services.Pagos;

public interface IPasarelaPago
{
    Task<ResultadoPasarela> ProcesarPagoAsync(
        string numeroTarjeta,
        decimal importe,
        CancellationToken ct = default);
}