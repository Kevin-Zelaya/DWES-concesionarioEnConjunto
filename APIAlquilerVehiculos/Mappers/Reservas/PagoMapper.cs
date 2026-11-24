using APIAlquilerVehiculos.DTOs.Pagos;

public static class PagoMapper
{
    /// <summary>
    /// Convierte una entidad Pago en su DTO de respuesta.
    /// </summary>
    /// <param name="pago">Pago que se desea convertir.</param>
    /// <returns>DTO con la información pública del pago.</returns>
    public static PagoResponse ToResponse(this Pago pago)
    {
        return new PagoResponse
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
    }
}