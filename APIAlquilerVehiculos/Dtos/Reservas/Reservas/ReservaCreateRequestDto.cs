using APIAlquilerVehiculos.Models.Enums;

public record ReservaCreateRequestDto(
    long ClienteId,
    CategoriaEnum Categoria,
    string VehiculoId,
    DateTimeOffset FechaRecogida,
    DateTimeOffset FechaDevolucion,
    List<LineaReservaCreateRequestDto> Lineas
);

public record LineaReservaCreateRequestDto(
    TipoLinea Tipo,
    long TipoId,
    int Cantidad
);