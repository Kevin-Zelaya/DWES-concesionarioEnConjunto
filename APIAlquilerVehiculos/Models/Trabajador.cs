namespace APIAlquilerVehiculos.Models;

/// <summary>
/// Representa un Trabajador de la empresa
/// </summary>
public class Trabajador
{
    /// <summary>
    /// Identificador único del trabajador
    /// </summary>
    public long Id { get; init; }
    
    /// <summary>
    /// Identificador del usuario asociado al trabajador.
    /// </summary>
    public long IdUsuario { get; init; }
    
    /// <summary>
    /// Cuenta de usuario asociada al trabajador.
    /// </summary>
    public Usuario Usuario { get; init; } = null!;
    
    /// <summary>
    /// Nombre del trabajador.
    /// </summary>
    public required string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Apellidos del trabajador
    /// </summary>
    public required string Apellidos { get; set; } = string.Empty;
    
    // Se quita Email y Telefono (PhoneNumber) ya que aparece en Usuario a traves de Identity
    
    /// <summary>
    /// Documento Nacional de Identidad del trabajador.
    /// </summary>
    public required string Dni { get; set; } 
    
    /// <summary>
    /// La fecha en el que el trabajador empezo a trabajar en la empresa
    /// </summary>
    public DateTime FechaIngreso { get; set; } 
    
    /// <summary>
    /// Tipo de trabajador según sus funciones: operaciones o mecánico.
    /// Operaciones o Mecanico
    /// </summary>
    public required Tipo Tipo { get; set; } 
    
    //Antes de eliminar seleccionar otro responsable
    /// <summary>
    /// Indica si el trabajador se encuentra activo en la empresa.
    /// </summary>
    public required bool Activo { get; set; } 
    
    /// <summary>
    /// Fecha de alta del trabajador
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// Fecha de modificación de datos del trabajador
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
    
    /// <summary>
    /// Fecha de baja logica del trabajador (si se produjó)
    /// </summary>
    public DateTime? DeletedAt { get; init; }
}
/// <summary>
/// Tipos de trabajadores según sus funciones dentro de la empresa.
/// </summary>
public enum Tipo
{
    /// <summary>
    /// Trabajador encargado de las tareas operativas.
    /// </summary>
    Operaciones,
    
    /// <summary>
    /// Trabajador encargado de las tareas de mantenimiento y reparación.
    /// </summary>
    Mecanico
}