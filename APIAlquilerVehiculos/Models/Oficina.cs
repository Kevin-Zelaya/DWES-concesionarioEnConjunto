using APIAlquilerVehiculos.Models.Usuarios;

namespace APIAlquilerVehiculos.Models;

/// <summary>
/// Representa la única oficina de la empresa
/// </summary>
public class Oficina
{
    /// <summary>
    /// Identificador de la oficina
    /// </summary>
    public long Id { get; init; }
    
    /// <summary>
    /// Nombre de la oficina
    /// </summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// Dirección de la oficina
    /// Propiedad embebida dentro de Oficina
    /// cuenta con Calle, codigo postal y la ciudad
    /// </summary>
    public Direccion Direccion { get; set; } = null!;
    
    /// <summary>
    /// Telefono de la oficina.
    /// </summary>
    public string Telefono { get; init; } = string.Empty;
    
    /// <summary>
    /// Localización de la oficina.
    /// Propiedad embebida dentro de Oficina.
    /// Cuenta con latitud y longitud.
    /// </summary>
    public Localizacion Localizacion { get; set; } = null!;
    
    /// <summary>
    /// Horario de atención de la oficina.
    /// Se almacena en formato HH:mm-HH:mm.
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
/// Dirección de la oficina. 
/// </summary>
public class Direccion
{
    /// <summary>
    /// Calle de la oficina
    /// </summary>
    public string Calle { get; init; } = string.Empty;
    
    /// <summary>
    /// Código postal de la oficna
    /// </summary>
    public string CodigoPostal { get; init; } = string.Empty;
    
    /// <summary>
    /// Ciudad donse se situa la oficina
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