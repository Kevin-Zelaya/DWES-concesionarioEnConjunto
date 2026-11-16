namespace APIAlquilerVehiculos.Models;

/// <summary>
/// Representa un Cliente que quiere alquilar un vehiculo
/// </summary>
public class Cliente
{
    /// <summary>
    /// Identificador único del cliente
    /// </summary>
    public long Id { get; init; }
    
    /// <summary>
    /// Identificador del usuario asociado al cliente.
    /// </summary>
    public long IdUsuario { get; init; }

    /// <summary>
    /// Cuenta de usuario asociada al cliente.
    /// </summary>
    public Usuario Usuario { get; init; } = null!;
    
    /// <summary>
    /// Nombre del Cliente
    /// </summary>
    public required string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Apellidos del cliente
    /// </summary>
    public required string Apellidos { get; set; } = string.Empty;
    
    // Se quita Email y Telefono (PhoneNumber) ya que aparece en Usuario a traves de Identity
    
    /// <summary>
    /// Fecha de nacimiento
    /// </summary>
    public DateTime FechaNacimiento { get; set; }
    
    /// <summary>
    /// Documento Nacional de Identidad del cliente, si se tiene.
    /// </summary>
    public string? Dni { get; set; } 
    
    /// <summary>
    /// Número de Identidad de Extranjero del cliente, si se tiene.
    /// </summary>
    public string? Nie { get; set; } 
    
    /// <summary>
    /// Número de pasaporte del cliente, si se tiene.
    /// </summary>
    public string? Pasaporte { get; set; } 
    
    /// <summary>
    /// nº de permiso de conducir, obligatorio si solo se facilita el pasaporte
    /// </summary>
    public string? NumeroPermisoConducir { get; set; }
    
    /// <summary>
    /// Fecha de alta del cliente.
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// Fecha de modificación de datos del cliente
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Indica si el cliente se encuentra activo en la empresa.
    /// </summary>
    public required bool IsActivo { get; set; } 
    
    /// <summary>
    /// Fecha de baja logica del cliente (si se produjó)
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}