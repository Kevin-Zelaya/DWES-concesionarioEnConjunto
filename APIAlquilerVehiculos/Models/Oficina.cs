namespace APIAlquilerVehiculos.Models;

/// <summary>
/// Representa la oficina de alquiler de vehículos.
/// </summary>
public class Oficina
{
    /// <summary>
    /// Identificador único de la oficina.
    /// </summary>
    public int Id { get; set; } = 0;
    
    /// <summary>
    /// Nombre de la oficina.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Dirección de la oficina.
    /// </summary>
    public string Direccion { get; set; } = string.Empty;
    
    /// <summary>
    /// Ciudad donde se encuentra la oficina.
    /// </summary>
    public string Ciudad { get; set; } = string.Empty;
    
    /// <summary>
    /// Código postal de la oficina.
    /// </summary>
    public int CodigoPostal { get; set; }
    
    /// <summary>
    /// Número de teléfono de la oficina.
    /// </summary>
    public string Telefono { get; set; } = string.Empty;
    
    /// <summary>
    /// Localización de la oficina.
    /// </summary>
    public string Localizacion { get; set; } = string.Empty;
    
    /// <summary>
    /// Horario de funcionamiento de la oficina.
    /// </summary>
    public string Horario { get; set; } = string.Empty;
    
    /// <summary>
    /// Identificador del responsable de la oficina.
    /// </summary>
    public long Responsable_Id { get; set; }
}