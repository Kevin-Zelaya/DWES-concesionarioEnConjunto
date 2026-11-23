using APIAlquilerVehiculos.Models.Reservas;

public static class ReservasMappers
{

    public static Reserva ToEntity(
        this ReservaCreateRequestDto dto,
        long? trabajadorId)
    {
        return new Reserva
        {
            ClienteId = dto.ClienteId,
            TrabajadorId = trabajadorId,
            Categoria = dto.Categoria,
            VehiculoId = dto.VehiculoId,
            FechaCreacion = DateTime.UtcNow,
            FechaRecogida = dto.FechaRecogida,
            FechaDevolucion = dto.FechaDevolucion,
            Estado = Estado.Pendiente,
            Lineas = [],
            PrecioTotal = 0m
        };
    }

    public static LineaReserva ToEntity(
        this LineaReservaCreateRequestDto dto)
    {
        return new LineaReserva
        {
            Tipo = dto.Tipo,
            Cantidad = dto.Cantidad,
            Concepto = string.Empty,
            PrecioUnitario = 0m,
            TipoPrecio = default,
            Subtotal = 0m
        };
    }

    public static ReservaDto ToDto(this Reserva reserva)
    {
        return new(
            reserva.Id,
            reserva.CodigoRefencia,
            reserva.ClienteId,
            reserva.TrabajadorId,
            reserva.Categoria,
            reserva.VehiculoId,
            reserva.FechaCreacion,
            reserva.FechaRecogida,
            reserva.FechaDevolucion,
            reserva.CalcularDiasAlquiler(),
            reserva.Estado,
            reserva.Lineas.Select(l => l.ToDto()).ToList(),
            reserva.PrecioTotal,
            reserva.Notas
        );
    }

    public static LineaReservaDto ToDto(this LineaReserva linea)
    {
        return new(
            linea.Id,
            linea.Tipo,
            linea.Concepto,
            linea.Cantidad,
            linea.PrecioUnitario,
            linea.TipoPrecio,
            linea.Subtotal
        );
    }
}