namespace APIAlquilerVehiculos.Models;

/// <summary>
/// Representa una marca de vehículos.
/// </summary>
public class Marca
{
    /// <summary>
    /// Identificador único de la marca.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Nombre de la marca.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// URL del sitio web de la marca.
    /// </summary>
    public string Wb { get; set; } = string.Empty;

   /// <summary>
    /// Indica el pais .
    /// </summary>
    public string Pais { get; set; } = string.Empty;

    
    /// <summary>
    /// Indica si la marca está activa o no.
    /// </summary>
    public bool Activo { get; set; }
    
    /// <summary>
    /// Fecha de creación de la marca.
    /// </summary>
    public DateTime? CreatedAt { get; set; }
    
    /// <summary>
    /// Fecha de última actualización de la marca.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Fecha de eliminación de la marca (si ha sido eliminada).
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}