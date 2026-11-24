
using System.ComponentModel.DataAnnotations;

namespace APIAlquilerVehiculos.DTOs.Pagos;


public class CrearPagoRequestDto
{
    public string NumeroTarjeta { get; set; } = string.Empty;
}