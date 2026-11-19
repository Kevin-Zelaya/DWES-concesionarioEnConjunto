using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using APIAlquilerVehiculos.Models.Usuarios;

namespace APIAlquilerVehiculos.Dtos.Trabajadores;

/// <summary>
/// Dto de entrada para modificar un trabajador
/// </summary>
/// <param name="Email">Email de acceso</param>
/// <param name="Telefono">Teléfono</param>
/// <param name="Nombre">Nombre del trabajador</param>
/// <param name="Apellidos">Apellidos del trabajador</param>
/// <param name="Dni">DNI del trabajador</param>
/// <param name="FechaIngreso">Fecha de ingreso en la empresa</param>
/// <param name="TipoTrabajador">Operaciones o Mecanico</param>
public record UpdateTrabajadorDto(

    [property: JsonPropertyName("email")]
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [MaxLength(255, ErrorMessage = "El email no puede superar los 255 caracteres")]
    string Email,

    [property: JsonPropertyName("telefono")]
    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    [MaxLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres")]
    string Telefono,

    [property: JsonPropertyName("nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    string Nombre,

    [property: JsonPropertyName("apellidos")]
    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [MinLength(2, ErrorMessage = "Los apellidos deben tener al menos 2 caracteres")]
    [MaxLength(150, ErrorMessage = "Los apellidos no pueden superar los 150 caracteres")]
    string Apellidos,

    [property: JsonPropertyName("dni")]
    [Required(ErrorMessage = "El DNI es obligatorio")]
    [MaxLength(9, ErrorMessage = "El DNI debe tener un máximo de 9 caracteres")]
    string Dni,

    [property: JsonPropertyName("fecha_ingreso")]
    [Required(ErrorMessage = "La fecha de ingreso es obligatoria")]
    DateTime FechaIngreso,

    [property: JsonPropertyName("tipo_trabajador")]
    [Required(ErrorMessage = "El tipo de trabajador es obligatorio")]
    [EnumDataType(typeof(TipoTrabajador), ErrorMessage = "El tipo de trabajador debe ser Operaciones o Mecanico")]
    TipoTrabajador TipoTrabajador
);