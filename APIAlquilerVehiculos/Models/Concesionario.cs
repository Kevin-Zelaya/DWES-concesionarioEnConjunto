namespace APIAlquilerVehiculos.Models;

/// <summary>
/// Representa un concesionario de vehículos.
/// </summary>
public class Concesionario
{
    /// <summary>
    /// Identificador único del concesionario.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Nombre del concesionario.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Ciudad donde se encuentra el concesionario.
    /// </summary>
    public string Ciudad { get; set; } = string.Empty;

    /// <summary>
    /// Número de teléfono del concesionario.
    /// </summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// Dirección de correo electrónico del concesionario.
    /// </summary>
    public string Correo { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica si el concesionario está activo o no.
    /// </summary>
    public bool Activo { get; set; }
    
    /// <summary>
    /// Fecha de creación del concesionario.
    /// </summary>
    public DateTime? CreatedAt { get; set; }
    
    /// <summary>
    /// Fecha de última actualización del concesionario.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Fecha de eliminación del concesionario (si ha sido eliminado).
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}