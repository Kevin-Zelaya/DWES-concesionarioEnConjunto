namespace APIAlquilerVehiculos.Models;

/// <summary>
/// Representa un modelo de vehículo.
/// </summary>
public class Modelo
{
    /// <summary>
    /// Obtiene o establece el identificador único del modelo.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Establece el nombre del modelo.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Establece la descripción del modelo.
    /// </summary>
    public string Descricion { get; set; } = string.Empty;

    /// <summary>
    /// Establece el código Acriss del modelo.
    /// </summary>
    public string Acriss { get; set; } = string.Empty;
    
    /// <summary>
    /// Indica si el modelo está activo o no.
    /// </summary>
    public bool Activo { get; set; }
    
    /// <summary>
    /// Fecha de creación del modelo.
    /// </summary>
    public DateTime? CreatedAt { get; set; }
    
    /// <summary>
    /// Fecha de última actualización del modelo.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Fecha de eliminación del modelo (si ha sido eliminado).
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}