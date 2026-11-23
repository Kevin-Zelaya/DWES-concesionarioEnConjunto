using APIAlquilerVehiculos.Models.Pagos;

namespace APIAlquilerVehiculos.Services.Pagos;

public class PasarelaPagoSimulada : IPasarelaPago
{
    public Task<ResultadoPasarela> ProcesarPagoAsync(
        string numeroTarjeta,
        decimal importe,
        CancellationToken ct = default)
    {
        bool aprobado = numeroTarjeta == "79927398713";

        var resultado = new ResultadoPasarela
        {
            Aprobado = aprobado,
            Referencia = Guid.NewGuid().ToString("N")
        };

        return Task.FromResult(resultado);
    }
}