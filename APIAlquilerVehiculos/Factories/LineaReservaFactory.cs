using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;

public class LineaReservaFactory(
    IPolizaRepository polizaRepository,
    IExtraRepository extraRepository,
    IRecargoRepository recargoRepository) : ILineaReservaFactory
{
    public async Task<Result<LineaReserva, DomainError>> CreateAsync(
        LineaReservaCreateRequestDto request,
        CancellationToken ct = default)
    {
        return request.Tipo switch
        {
            TipoLinea.Poliza => await CrearPolizaAsync(request, ct),
            TipoLinea.Extra => await CrearExtraAsync(request, ct),
            TipoLinea.Recargo => await CrearRecargoAsync(request, ct),

            _ => Result.Failure<LineaReserva, DomainError>(
                NotFoundError.FromResource("Tipo de linea" , "0"))
        };
    }

    // POLIZA
    private async Task<Result<LineaReserva, DomainError>> CrearPolizaAsync(
        LineaReservaCreateRequestDto request,
        CancellationToken ct)
    {
        var poliza = await polizaRepository.GetByIdAsync(request.TipoId, ct);

        if (poliza is null)
            return Result.Failure<LineaReserva, DomainError>(
                NotFoundError.FromResource("Poliza" ,request.TipoId.ToString()));

        return Result.Success<LineaReserva, DomainError>(new LineaReserva
        {
            Tipo = TipoLinea.Poliza,
            Concepto = poliza.Nombre,
            Cantidad = request.Cantidad,
            PrecioUnitario = poliza.IncrementoDiario,
            TipoPrecio = TipoPrecio.Diario
        });
    }

    // EXTRA
    private async Task<Result<LineaReserva, DomainError>> CrearExtraAsync(
        LineaReservaCreateRequestDto request,
        CancellationToken ct)
    {
        var extra = await extraRepository.GetByIdAsync(request.TipoId, ct);

        if (extra is null)
            return Result.Failure<LineaReserva, DomainError>(
                NotFoundError.FromResource("Extra" ,request.TipoId.ToString()));

        return Result.Success<LineaReserva, DomainError>(new LineaReserva
        {
            Tipo = TipoLinea.Extra,
            Concepto = extra.Nombre,
            Cantidad = request.Cantidad,
            PrecioUnitario = extra.Precio,
            TipoPrecio = extra.TipoPrecio
        });
    }

    // RECARGO
    private async Task<Result<LineaReserva, DomainError>> CrearRecargoAsync(
        LineaReservaCreateRequestDto request,
        CancellationToken ct)
    {
        var recargo = await recargoRepository.GetByIdAsync(request.TipoId, ct);

        if (recargo is null)
            return Result.Failure<LineaReserva, DomainError>(
                NotFoundError.FromResource("Recargo" ,request.TipoId.ToString()));

        return Result.Success<LineaReserva, DomainError>(new LineaReserva
        {
            Tipo = TipoLinea.Recargo,
            Concepto = recargo.Nombre,
            Cantidad = request.Cantidad,
            PrecioUnitario = recargo.Importe,
            TipoPrecio = recargo.TipoPrecio
        });
    }
}