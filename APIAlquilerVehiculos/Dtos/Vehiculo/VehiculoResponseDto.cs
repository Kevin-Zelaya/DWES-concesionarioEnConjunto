using System;
using APIAlquilerVehiculos.Models.Enums;

namespace APIAlquilerVehiculos.Dtos.Vehiculo;

/// <summary>
/// Objeto de transferencia de respuesta de vehiculo
/// </summary>
public class VehiculoResponseDto
{
    public string Id { get; set; } = string.Empty;
    
    public string Matricula { get; set; } = string.Empty;

    public long MarcaId { get; set; }

    public string Modelo { get; set; } = string.Empty;

    public string Acriss { get; set; } = string.Empty;

    public CategoriaEnum Categoria { get; set; }

    public long ConcesionarioId { get; set; }

    public string Color { get; set; } = string.Empty;

    public string Combustible { get; set; } = string.Empty;

    public double Maletero { get; set; }

    public int NumeroPlazas { get; set; }

    public double Autonomia { get; set; }

    public long Imagenes { get; set; }

    public EstadoEnum Estado { get; set; }

    public double PrecioPorDia { get; set; }

    public DateTime UltimaRevision { get; set; }

    public bool Activo { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}