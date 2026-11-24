using APIAlquilerVehiculos.DTOs.Pagos;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Models.Reservas;

public record ReservaDto(
    long Id,
    string CodigoRefencia,
    long ClienteId,
    long? TrabajadorId,
    CategoriaEnum Categoria,
    string VehiculoId,
    DateTime FechaCreacion,
    DateTime FechaRecogida,
    DateTime FechaDevolucion,
    int DiasAlquiler,
    Estado Estado,
    List<LineaReservaDto> Lineas,
    List<PagoResponse> Pagos,
    decimal PrecioTotal,
    string? Notas
);

public record LineaReservaDto(
    long Id,
    TipoLinea Tipo,
    string Concepto,
    int Cantidad,
    decimal PrecioUnitario,
    TipoPrecio TipoPrecio,
    decimal Subtotal
);