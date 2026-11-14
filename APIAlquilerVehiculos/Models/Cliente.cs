namespace APIAlquilerVehiculos.Models;

public class Cliente
{
    public long Id { get; init; }
    
    public long IdUsuario { get; init; }

    public Usuario Usuario { get; init; } = null!;
    
    public required string Nombre { get; set; } = string.Empty;
    
    public required string Apellidos { get; set; } = string.Empty;
    
    // Se quita Email y Telefono (PhoneNumber) ya que aparece en Usuario a traves de Identity
    
    public DateTime FechaNacimiento { get; set; }
    
    public string? Dni { get; set; } 
    
    public string? Nie { get; set; } 
    
    public string? Pasaporte { get; set; } 
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime? UpdatedAt { get; init; }
    
    public required bool Activo { get; set; } 
    
    public DateTime? DeletedAt { get; init; }
}