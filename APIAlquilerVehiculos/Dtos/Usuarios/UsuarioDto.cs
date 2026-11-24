using APIAlquilerVehiculos.Models.Usuarios;
using Swashbuckle.AspNetCore.Annotations;

namespace APIAlquilerVehiculos.Dtos.Usuarios;

public record UsuarioDto
(
    [property: SwaggerSchema(Description = "Identificador del usuario", Format = "long")]
    long Id,
    
    [property: SwaggerSchema(Description = "Correo electronico del usuario")]
    string Email,
    
    [property: SwaggerSchema(Description = "Nombre del usuario")]
    string Nombre,
    
    [property: SwaggerSchema(Description = "Apellidos del usuario")]
    string Apellidos,
    
    [property: SwaggerSchema(Description = "Telefono del usuario")]
    string? Telefono,
    
    [property: SwaggerSchema(Description = "Rol del usuario")]
    string Rol,
    
    [property: SwaggerSchema(Description = "Tipo de usuario")]
    string TipoUsuario,
    
    [property: SwaggerSchema(Description = "Marcador de baja logica del tipo de usuario", Format = "bool")]
    bool IsActivo,
    
    [property: SwaggerSchema(Description = "Fecha de registro en formato UTC de creación del usuario", Format = "DateTime")]
    DateTime CreatedAt
);

public record UsuarioDetalleDto
(
    [property: SwaggerSchema(Description = "Identificador del usuario")]
    long Id,
    
    [property: SwaggerSchema(Description = "Correo electronico del usuario")]
    string Email,
    
    [property: SwaggerSchema(Description = "Nombre y apellidos del usuario")]
    string NombreYApellidos,
    
    [property: SwaggerSchema(Description = "Telefono del usuario")]
    string? Telefono,
    
    [property: SwaggerSchema(Description = "Rol del usuario", Format = "DateTime")]
    string Rol,
    
    [property: SwaggerSchema(Description = "Tipo de usuario", Format = "DateTime")]
    string TipoUsuario,
    
    [property: SwaggerSchema(Description = "Marcador de baja logica del tipo de usuario", Format = "bool")]
    bool IsActivo,
    
    [property: SwaggerSchema(Description = "Fecha de registro en formato UTC de creación del usuario", Format = "DateTime")]
    DateTime CreatedAt,
    
    [property: SwaggerSchema(Description = "Fecha de registro en formato UTC de modificacón del usuario", Format = "DateTime")]
    DateTime? UpdatedAt
);