namespace APIAlquilerVehiculos.Models;

/// <summary>
/// Representa un concesionario de vehículos.
/// </summary>
public class Concesionario
{
    /// <summary>
    /// Identificador único del concesionario.
    /// </summary>
    public int Id { get; set; }

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
}