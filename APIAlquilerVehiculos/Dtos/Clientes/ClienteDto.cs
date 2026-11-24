using Swashbuckle.AspNetCore.Annotations;

namespace APIAlquilerVehiculos.Dtos.Cientes;

[SwaggerSchema(Description = "Datos del cliente.")]
public record ClienteDto (

    [property: SwaggerSchema(Description = "Identificador de un cliente")]
    long Id,
    
    [property: SwaggerSchema(Description = "Identificador del usuario asociado", Format = "long")]
    long IdUsuario,
    
    [property: SwaggerSchema(Description = "Nombre del cliente")]
    string Nombre,
    
    [property: SwaggerSchema(Description = "Apellidos del cliente")]
    string Apellidos,
    
    [property: SwaggerSchema(Description = "Correo electronico del cliente")]
    string Email,
    
    [property: SwaggerSchema(Description = "Telefono del cliente")]
    string? Telefono,
    
    [property: SwaggerSchema(Description = "Fecha de nacimiento del cliente", Format = "DateTime")]
    DateTime FechaNacimiento,
    
    [property: SwaggerSchema(Description = "Documento Nacional de Identidad del cliente", Nullable = true)]
    string? Dni,
    
    [property: SwaggerSchema(Description = "Número de Identidad de Extranjero del cliente", Nullable = true)]
    string? Nie,
    
    [property: SwaggerSchema(Description = "Número de pasaporte del cliente", Nullable = true)]
    string? Pasaporte,
    
    [property: SwaggerSchema(Description = "Número de permiso de conducir del cliente", Nullable = true)]
    string? NumeroPermisoConducir,
    
    [property: SwaggerSchema(Description = "Fecha de registro en formato UTC de creación del cliente", Format = "DateTime")]
    DateTime CreatedAt,
    
    [property: SwaggerSchema(Description = "Fecha de registro en formato UTC de actualización del cliente", Format = "DateTime", Nullable = true)]
    DateTime? UpdatedAt,
    
    [property: SwaggerSchema(Description = "Marcador de baja logica del cliente", Format = "bool")]
    bool IsActivo,
    
    [property: SwaggerSchema(Description = "Fecha de registro en formato UTC de eliminación del cliente", Format = "DateTime" , Nullable = true)]
    DateTime? DeletedAt
);