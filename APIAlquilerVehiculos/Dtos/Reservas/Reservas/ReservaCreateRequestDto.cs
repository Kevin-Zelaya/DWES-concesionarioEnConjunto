using APIAlquilerVehiculos.Models.Enums;

public record ReservaCreateRequestDto(
    long ClienteId,
    long EmpleadoId, // quitar posteriormente
    CategoriaEnum Categoria,
    string VehiculoId,
    DateTime FechaRecogida,
    DateTime FechaDevolucion,
    List<LineaReservaCreateRequestDto> Lineas
);

public record LineaReservaCreateRequestDto(
    TipoLinea Tipo,
    long TipoId,
    int Cantidad
);