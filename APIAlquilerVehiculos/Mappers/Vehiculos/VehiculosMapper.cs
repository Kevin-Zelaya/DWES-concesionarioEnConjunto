using APIAlquilerVehiculos.Dtos.Vehiculo;
using APIAlquilerVehiculos.Models;


namespace APIAlquilerVehiculos.Mappers.Vehiculos;

public static class VehiculoMapper
{
    public static VehiculoResponseDto ToResponseDto(this Vehiculo vehiculo)
    {
        return new VehiculoResponseDto
        {
            Id = vehiculo.Id,
            Matricula = vehiculo.Matricula,
            MarcaId = vehiculo.MarcaId,
            Modelo = vehiculo.Modelo,
            Acriss = vehiculo.Acriss,
            Categoria = vehiculo.categoria,
            ConcesionarioId = vehiculo.Concesionario_id,
            Color = vehiculo.Color,
            Combustible = vehiculo.Combustible,
            Maletero = vehiculo.Maletero,
            NumeroPlazas = vehiculo.NumeroPlazas,
            Autonomia = vehiculo.Autonomia,
            Imagenes = vehiculo.Imagenes,
            Estado = vehiculo.Estado,
            PrecioPorDia = vehiculo.precioPorDia,
            UltimaRevision = vehiculo.UltimaRevision,
            Activo = vehiculo.Activo,
            CreatedAt = vehiculo.CreatedAt,
            UpdatedAt = vehiculo.UpdatedAt
        };
    }

    public static Vehiculo ToVehiculo(this VehiculoCreateDto dto)
    {
        return new Vehiculo
        {
            Matricula = dto.Matricula,
            MarcaId = dto.MarcaId,
            Modelo = dto.Modelo,
            Acriss = dto.Acriss,
            categoria = dto.Categoria,
            Concesionario_id = dto.ConcesionarioId,
            Color = dto.Color,
            Combustible = dto.Combustible,
            Maletero = dto.Maletero,
            NumeroPlazas = dto.NumeroPlazas,
            Autonomia = dto.Autonomia,
            Imagenes = dto.Imagenes,
            Estado = dto.Estado,
            precioPorDia = (decimal)dto.PrecioPorDia,
            UltimaRevision = dto.UltimaRevision,
            Activo = dto.Activo
        };
    }

    public static Vehiculo ToVehiculo(this VehiculoUpdateDto dto)
    {
        return  new Vehiculo()
        {
            Matricula = dto.Matricula,
            MarcaId = dto.MarcaId,
            Modelo = dto.Modelo,
            Acriss = dto.Acriss,
            categoria = dto.Categoria,
            Concesionario_id = dto.ConcesionarioId,
            Color = dto.Color,
            Combustible = dto.Combustible,
            Maletero = dto.Maletero,
            NumeroPlazas = dto.NumeroPlazas,
            Autonomia = dto.Autonomia,
            Imagenes = dto.Imagenes,
            Estado = dto.Estado,
            precioPorDia = dto.PrecioPorDia,
            UltimaRevision = dto.UltimaRevision,
            Activo = dto.Activo
        };
        
    }
}