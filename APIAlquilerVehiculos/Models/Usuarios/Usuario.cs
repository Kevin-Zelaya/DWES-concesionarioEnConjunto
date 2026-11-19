using Microsoft.AspNetCore.Identity;

namespace APIAlquilerVehiculos.Models.Usuarios;

/// <summary>
/// Representa un usuario de la API.
/// </summary>
public class Usuario : IdentityUser<long>
{
    // Id, Email, ClaveHash y Roles ya vienen por defecto en Idenity
    
    /// <summary>
    /// Perfil de trabajador asociado a este usuario, si existe.
    /// </summary>
    public Trabajador? Trabajador { get; set; }
    
    /// <summary>
    /// Perfil de cliente asociado a este usuario, si existe.
    /// </summary>
    public Cliente? Cliente { get; set; }
    
    /// <summary>
    /// Fecha de alta de usuario
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// Fecha de modificación de usuario
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Roles de autorización la API
/// </summary>
public static class Roles
{
    // Permiso Publico => Sin autenticar
    // Mas trarde se configura
    
    /// <summary>
    /// Usuario con permisos de administración.
    /// </summary>
    public const string Administrador = "Administrador";
    
    /// <summary>
    /// Usuario que puede realizar operaciones como cliente.
    /// </summary>
    public const string Cliente = "Cliente";
    
    /// <summary>
    /// Trabajador del tipo operario.
    /// </summary>
    public const string Operario = "Operario";
    
    /// <summary>
    /// Trabajador del tipo mecánico.
    /// </summary>
    public const string Mecanico = "Mecanico";
    
    /// <summary>
    /// Trabajador responsable de Oficina
    /// </summary>
    public const string Responsable = "Responsable";
    
    // para el seeder
    public static readonly string[] Todos = { Cliente, Operario, Mecanico, Responsable, Administrador };
}