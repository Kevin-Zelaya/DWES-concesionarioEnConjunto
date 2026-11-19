using APIAlquilerVehiculos.Models.Usuarios;

namespace APIAlquilerVehiculos.Models;

public class Parte
{
    public long Id { get; set; }
    public DateTime FechaApertura { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCierre { get; set; }
    public long QuienLoTramitaId { get; set; }
    public Trabajador? QuienLoTramita { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string VehiculoId { get; set; } = string.Empty;
    public Vehiculo? Vehiculo { get; set; }
    public EstadoParte Estado { get; set; } = EstadoParte.Abierto;
}

public enum EstadoParte
{
    Abierto,
    Cerrado
}