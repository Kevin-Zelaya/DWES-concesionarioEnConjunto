namespace APIAlquilerVehiculos.Models;

/// <summary>
/// Representa una marca de vehículos.
/// </summary>
public class Marca
{
    /// <summary>
    /// Identificador único de la marca.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre de la marca.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// URL del sitio web de la marca.
    /// </summary>
    public string Wb { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica si la marca está activa o no.
    /// </summary>
    public bool Activo { get; set; }
}