using System.ComponentModel.DataAnnotations;
using APIAlquilerVehiculos.Models.Reservas;

/// <summary>
/// Permite actualizar el estado de una reserva
/// mediante el código de referencia
/// </summary>
/// <param name="Estado"></param>
/// <param name="RowVersion"></param>
public record ReservaUpdatePagoRequestDto(
    long PagoId,
    [property: Required]
    uint RowVersion
);