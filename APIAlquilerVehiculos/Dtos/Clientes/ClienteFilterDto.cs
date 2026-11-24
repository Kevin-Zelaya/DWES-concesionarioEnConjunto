using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace APIAlquilerVehiculos.Dtos.Cientes;

[SwaggerSchema(Description = "Filtros para obtener una lista de cientes")]
public record ClienteFilterDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre del ciente no puede superar los 100 caracteres y debe ser superior a 2")]
    [SwaggerSchema(Description = "Nombre del ciente", Nullable = true)]
    public string? Nombre { get; init; } 
    
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Los apellidos del ciente no puede superar los 150 caracteres y debe ser superior a 2")]
    [SwaggerSchema(Description = "Apellidos del funko", Nullable = true)]
    public string? Apellidos  { get; init; } 
    
    [EmailAddress(ErrorMessage = "El formato del email no es válido")] 
    [StringLength(255, MinimumLength = 2, ErrorMessage = "Los apellidos del ciente no puede superar los 255 caracteres y debe ser superior a 2")]
    [SwaggerSchema(Description = "Email del cliente", Nullable = true)]
    public string? Email { get; init; }
    
    [SwaggerSchema(Description = "Marcador de cliente activo", Format = "bool", Nullable = true)]
    public bool? IsActivo { get; init; }
    
    
    [Required (ErrorMessage =  "El número de pagina es obligatorio")]
    [Range(0, int.MaxValue, ErrorMessage = "La página no puede ser negativa")]
    [SwaggerSchema(Description = "Número de pagina", Format = "int", Nullable = true)]
    public int Page { get; init; } = 0;

    [Required (ErrorMessage =  "El número de elementos en la pagina es obligatorio")]
    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100")]
    [SwaggerSchema(Description = "Número de funkos en una pagina", Format = "int", Nullable = true)]
    public int Size { get; init; } = 10;
    
    // <summary>
    // Campo para ordenar resultados.
    // </summary>
    // <default>id</default>
    // <example>precio</example>
    public string SortBy = "id";

    // <summary>
    // Dirección de ordenamiento (asc/desc).
    // </summary>
    // <default>asc</default>
    // <example>desc</example>
    public string Direction = "asc";
}