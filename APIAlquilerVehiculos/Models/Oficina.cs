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
    /// Horario de atención.
    /// Si se excede del horario, oficina cerrada.
    /// </summary>
    public string Horario { get; init; } = string.Empty;
    
    /// <summary>
    /// Identificador del trabajador responsable.
    /// </summary>
    public long IdResponsable { get; init; } // Id del trabajador responsable
    
    /// <summary>
    /// Trabajador responsable.
    /// </summary>
    public Trabajador Responsable { get; set; } = null!; // acceder a Trabajador
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
/// Localización de la oficina
/// Representada con Latitud y longitud
/// </summary>
public class Localizacion
{
    /// <summary>
    /// Latitud de la oficina
    /// </summary>
    public decimal Latitud { get; set; }

    /// <summary>
    /// Longitud de la oficina
    /// </summary>
    public decimal Longitud { get; set; }
}