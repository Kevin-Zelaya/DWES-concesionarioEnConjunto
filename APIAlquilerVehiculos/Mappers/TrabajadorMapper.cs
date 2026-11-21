using APIAlquilerVehiculos.Dtos.Trabajadores;
using APIAlquilerVehiculos.Models.Usuarios;

namespace APIAlquilerVehiculos.Mappers;

public static class TrabajadorMapper
{
    /// <summary>
    /// trabajador -> response. necesita user para email y tlf
    /// </summary>
    public static ResponseTrabajadorDto ToResponseDto(this Trabajador t) => new(
        Id: t.Id,
        IdUsuario: t.IdUsuario,
        Nombre: t.Nombre,
        Apellidos: t.Apellidos,
        Email: t.Usuario?.Email ?? string.Empty,
        Telefono: t.Usuario?.PhoneNumber,
        Dni: t.Dni,
        FechaIngreso: t.FechaIngreso,
        TipoTrabajador: t.TipoTrabajador.ToString(),
        IsActivo: t.IsActivo,
        CreatedAt: t.CreatedAt,
        UpdatedAt: t.UpdatedAt
    );

    /// <summary>
    /// createdto -> trabajador. el user de identity tiene que estar creado
    /// </summary>
    public static Trabajador ToEntity(this CreateTrabajadorDto dto, Usuario usuario) => new()
    {
        IdUsuario = usuario.Id,
        Usuario = usuario,
        Nombre = dto.Nombre.Trim(),
        Apellidos = dto.Apellidos.Trim(),
        Dni = NormalizarDocumento(dto.Dni),
        FechaIngreso = ToUtc(dto.FechaIngreso ?? DateTime.UtcNow),
        TipoTrabajador = dto.TipoTrabajador,
        IsActivo = true, // un trabajador nuevo nace siempre activo
        CreatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// updatedto -> trabajador
    /// </summary>
    public static Trabajador UpdateEntity(this UpdateTrabajadorDto dto, long id) => new()
    {
        Id = id,
        Nombre = dto.Nombre.Trim(),
        Apellidos = dto.Apellidos.Trim(),
        Dni = NormalizarDocumento(dto.Dni),
        FechaIngreso = ToUtc(dto.FechaIngreso),
        TipoTrabajador = dto.TipoTrabajador,
        IsActivo = true // ignorado por el repositorio
    };

    /// <summary>
    /// rol identity que le corresponde a cada tipo de trabajador
    /// </summary>
    public static string ToRol(this TipoTrabajador tipo) => tipo switch
    {
        TipoTrabajador.Operaciones => Roles.Operario,
        TipoTrabajador.Mecanico => Roles.Mecanico,
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de trabajador no soportado")
    };

    // dni siempre en mayusc. y sin espacios, para que el índice único funcione bien
    private static string NormalizarDocumento(string doc) => doc.Trim().ToUpperInvariant();

    private static DateTime ToUtc(DateTime fecha) => fecha.Kind switch
    {
        DateTimeKind.Utc => fecha,
        DateTimeKind.Local => fecha.ToUniversalTime(),
        _ => DateTime.SpecifyKind(fecha, DateTimeKind.Utc)
    };
}