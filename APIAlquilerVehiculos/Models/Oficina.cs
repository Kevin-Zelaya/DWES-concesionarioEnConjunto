namespace APIAlquilerVehiculos.Models;

/// <summary>
/// Representa una oficina de alquiler de vehículos.
/// </summary>
public class Oficina
{
    /// <summary>
    /// Obtiene o establece el identificador único de la oficina.
    /// </summary>
    public long Id { get; init; }
    
    /// <summary>
    /// Obtiene o establece el nombre de la oficina.
    /// </summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// Obtiene o establece la dirección de la oficina.
    /// </summary>
    public Direccion Direccion { get; set; } = null!;
    
    /// <summary>
    /// Obtiene o establece el número de teléfono de la oficina.
    /// </summary>
    public string Telefono { get; init; } = string.Empty;
    
    /// <summary>
    /// Obtiene o establece la localización de la oficina.
    /// </summary>
    public Localizacion Localizacion { get; set; } = null!;
    
    /// <summary>
    /// Obtiene o establece el horario de la oficina.
    /// </summary>
    public string Horario { get; init; } = string.Empty;
    
    /// <summary>
    /// Obtiene o establece el identificador del trabajador responsable de la oficina.
    /// </summary>
    public long IdResponsable { get; init; } // Id del trabajador responsable
    
    /// <summary>
    /// Obtiene o establece el trabajador responsable de la oficina.
    /// </summary>
    public Trabajador Responsable { get; set; } = null!;
    
    /// <summary>
    /// Indica si la oficina está activa o no.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Obtiene o establece la fecha de última actualización de la oficina.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Indica si el modelo está activo o no.
    /// </summary>
    public bool Activo { get; set; }
    
    /// <summary>
    /// Fecha de eliminación del modelo (si ha sido eliminado).
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}

// OwnsOne
/// <summary>
/// Representa la dirección de una oficina.
/// </summary>
public class Direccion
{
    /// <summary>
    /// Obtiene o establece la calle de la dirección.
    /// </summary>
    public string Calle { get; init; } = string.Empty;
    /// <summary>
    /// Obtiene o establece el código postal de la dirección.
    /// </summary>
    public string CodigoPostal { get; init; } = string.Empty;
    /// <summary>
    /// Obtiene o establece la ciudad de la dirección.
    /// </summary>
    public string Ciudad { get; set; } = string.Empty;
}

/// <summary>
/// Representa la localización geográfica de una oficina.
/// </summary>
public class Localizacion
{
    /// <summary>
    /// Obtiene o establece la latitud de la localización.
    /// </summary>
    public decimal Latitud { get; set; }

    /// <summary>
    /// Obtiene o establece la longitud de la localización.
    /// </summary>
    public decimal Longitud { get; set; }
}