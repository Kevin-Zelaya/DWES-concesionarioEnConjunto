namespace APIAlquilerVehiculos.DTOs.Pagos;

public class PagoResponse
{
    public long Id { get; set; }

    public long ReservaId { get; set; }

    public decimal Importe { get; set; }

    public string Divisa { get; set; } = "EUR";

    public EstadoPago Estado { get; set; }

    public MarcaTarjeta MarcaTarjeta { get; set; }

    public DateTime Fecha { get; set; }

    public string UltimosCuatroDigitos { get; set; } = string.Empty;
}