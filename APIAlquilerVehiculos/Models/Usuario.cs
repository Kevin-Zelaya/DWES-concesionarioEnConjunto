using Microsoft.AspNetCore.Identity;

namespace APIAlquilerVehiculos.Models;

public class Usuario : IdentityUser<long>
{
    // Id, Email, ClaveHash y Roles ya vienen por defecto en Idenity
    
    public Trabajador? Trabajador { get; init; }
    
    public Cliente? Cliente { get; init; }
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime? UpdatedAt { get; init; }
}

public static class Roles
{
    // Publico => Sin autenticar
    // Mas trarde se configura
    public const string Administrador = "Administrador";
    public const string Cliente = "Cliente";
    public const string Trabajador = "Trabajador";
    public const string Responsable = "Responsable";
}