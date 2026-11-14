namespace APIAlquilerVehiculos.Models;

public class Trabajador
{
    public long Id { get; init; }
    
    public long IdUsuario { get; init; }
    
    public Usuario Usuario { get; init; } = null!;
    
    public required string Nombre { get; set; } = string.Empty;
    
    public required string Apellidos { get; set; } = string.Empty;
    
    // Se quita Email y Telefono (PhoneNumber) ya que aparece en Usuario a traves de Identity
    
    public required string Dni { get; set; } 
    
    public DateTime FechaIngreso { get; set; } 
    
    public required Tipo Tipo { get; set; } 
    
    //Antes de eliminar seleccionar otro responsable
    public required bool Activo { get; set; } 
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime? UpdatedAt { get; init; }
    
    public DateTime? DeletedAt { get; init; }
}

public enum Tipo
{
    Operaciones,
    Mecanico
}