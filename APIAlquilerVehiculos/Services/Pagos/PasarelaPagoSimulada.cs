using APIAlquilerVehiculos.Models.Pagos;

namespace APIAlquilerVehiculos.Services.Pagos;

public class PasarelaPagoSimulada : IPasarelaPago
{
    public Task<ResultadoPasarela> ProcesarPagoAsync(
        string numeroTarjeta,
        decimal importe,
        CancellationToken ct = default)
    {
        var resultado = new ResultadoPasarela
        {
            Aprobado = new Random().Next(0, 2) == 1,
            Referencia = Guid.NewGuid().ToString("N")
        };

        return Task.FromResult(resultado);
    }
}