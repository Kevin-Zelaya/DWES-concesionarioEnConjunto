using System.Runtime.CompilerServices;
using APIAlquilerVehiculos.Dtos.Cientes;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Validators.Auth.Documentos;

namespace APIAlquilerVehiculos.Mappers;

public static class ClienteMapper
{
     /// <summary>
    /// Cliente -> response. necesita
    /// </summary>
    public static ClienteDto ToResponseDto(this Cliente c) => new(
        Id: c.Id,
        IdUsuario: c.IdUsuario,
        Nombre: c.Nombre,
        Apellidos: c.Apellidos,
        Email: c.Usuario?.Email ?? string.Empty,
        Telefono: c.Usuario?.PhoneNumber,
        FechaNacimiento: c.FechaNacimiento,
        Dni: c.Dni,
        Nie: c.Nie,
        Pasaporte: c.Pasaporte,
        NumeroPermisoConducir: c.NumeroPermisoConducir,
        CreatedAt: c.CreatedAt,
        UpdatedAt: c.UpdatedAt,
        IsActivo: c.IsActivo,
        DeletedAt: c.DeletedAt
    );
     
    /// <summary>
    /// updatedto -> cliente
    /// </summary>
    public static Cliente UpdateEntity(this UpdateClienteDto dto, long id) => new()
    {
        Id = id,
        Nombre = dto.Nombre.Trim(),
        Apellidos = dto.Apellidos.Trim(),
        FechaNacimiento = dto.FechaNacimiento,
        Dni = NormalizarDocumento(dto.Dni),
        Nie = NormalizarDocumento(dto.Nie),
        Pasaporte = NormalizarDocumento(dto.Pasaporte),
        NumeroPermisoConducir =  NormalizarDocumento(dto.NumeroPermisoConducir),
        IsActivo = true // ignorado por el repositorio
    };
    
    public static List<ClienteDto> ToDtoList(this IEnumerable<Cliente> cliente) =>
        cliente.Select(f => f.ToResponseDto()).ToList();

    // Documentos de identidad siempre en mayusculas y sin espacios, para que el índice único funcione bien
    private static string NormalizarDocumento(string doc) => doc.Trim().ToUpperInvariant();

    // Npgsql (timestamptz) solo acepta DateTime con Kind = Utc
    private static DateTime ToUtc(DateTime fecha) => fecha.Kind switch
    {
        DateTimeKind.Utc => fecha,
        DateTimeKind.Local => fecha.ToUniversalTime(),
        _ => DateTime.SpecifyKind(fecha, DateTimeKind.Utc)
    };

}