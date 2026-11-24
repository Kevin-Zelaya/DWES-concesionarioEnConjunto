
using APIAlquilerVehiculos.DTOs.Pagos;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Errors.Pagos;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Repositories.Pagos;
using CSharpFunctionalExtensions;

namespace APIAlquilerVehiculos.Services.Pagos;

public class PagoService(
    IPagoRepository _repository,
    IPasarelaPago _pasarela,
    ICifradorTarjeta _cifrador,
    IReservaService _ReservaService
) : IPagoService
{
    public async Task<Result<PagoResponse, DomainError>> CrearPagoAsync(
        long reservaId,
        CrearPagoRequestDto request,
        CancellationToken ct = default)
    {
        var reserva = await _repository.GetReservaByIdAsync(reservaId, ct);

        if (reserva is null)
            return PagoErrors.ReservaNotFound(reservaId);

        if (reserva.Estado != Estado.Pendiente)
            return PagoErrors.ReservaNoPendiente();

        var numeroTarjeta = request.NumeroTarjeta.Replace(" ", "");

        var resultado = await _pasarela.ProcesarPagoAsync(
            numeroTarjeta,
            reserva.PrecioTotal,
            ct);

        var pago = new Pago
        {
            ReservaId = reserva.Id,
            Importe = reserva.PrecioTotal,
            Divisa = "EUR",
            Estado = resultado.Aprobado
                ? EstadoPago.Aprobado
                : EstadoPago.Rechazado,
            MarcaTarjeta = ObtenerMarca(numeroTarjeta),
            Fecha = DateTime.UtcNow,
            UltimosCuatroDigitos = numeroTarjeta[^4..],
            TarjetaCifrada = _cifrador.Cifrar(numeroTarjeta),
            ReferenciaPasarela = resultado.Referencia
        };

        if (resultado.Aprobado)
        {
            await _ReservaService.UpdateStatusAsync(reserva.CodigoRefencia, new ReservaUpdateStatusRequestDto(
                Estado: Estado.Confirmada,
                RowVersion: reserva.RowVersion
            ),
            ct);
        }

        await _repository.AddPagoAsync(pago, ct);

        var response = new PagoResponse
        {
            Id = pago.Id,
            ReservaId = pago.ReservaId,
            Importe = pago.Importe,
            Divisa = pago.Divisa,
            Estado = pago.Estado,
            MarcaTarjeta = pago.MarcaTarjeta,
            Fecha = pago.Fecha,
            UltimosCuatroDigitos = pago.UltimosCuatroDigitos
        };

        return Result.Success<PagoResponse, DomainError>(response);
    }

    private static MarcaTarjeta ObtenerMarca(string numero)
    {
        if (numero.StartsWith('4'))
            return MarcaTarjeta.Visa;

        if (numero.StartsWith("51") || numero.StartsWith("52") ||
            numero.StartsWith("53") || numero.StartsWith("54") ||
            numero.StartsWith("55"))
            return MarcaTarjeta.Mastercard;

        return MarcaTarjeta.Otra;
    }
}
