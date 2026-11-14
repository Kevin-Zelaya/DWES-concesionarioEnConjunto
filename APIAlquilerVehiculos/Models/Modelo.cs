namespace APIAlquilerVehiculos.Models;

/// <summary>
/// Representa un modelo de vehículo.
/// </summary>
public class Modelo
{
    /// <summary>
    /// Obtiene o establece el identificador único del modelo.
    /// </summary>
    public int Id { get; set; }
    
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
}