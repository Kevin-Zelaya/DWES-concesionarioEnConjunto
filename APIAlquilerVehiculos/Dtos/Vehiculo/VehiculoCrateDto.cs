using System;
using APIAlquilerVehiculos.Models.Enums;

namespace APIAlquilerVehiculos.Dtos.Vehiculo;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Objeto de tranferencia de creacion de vehiculos
/// </summary>
public class VehiculoCreateDto
{
    [Required]
    public string Matricula { get; set; } = string.Empty;

    [Required]
    public long MarcaId { get; set; }

    [Required]
    public string Modelo { get; set; } = string.Empty;

    [Required]
    public string Acriss { get; set; } = string.Empty;

    [Required]
    public CategoriaEnum Categoria { get; set; }

    [Required]
    public long ConcesionarioId { get; set; }

    [Required]
    public string Color { get; set; } = string.Empty;

    [Required]
    public string Combustible { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public double Maletero { get; set; }

    [Range(1, 20)]
    public int NumeroPlazas { get; set; }

    [Range(0, double.MaxValue)]
    public double Autonomia { get; set; }

    public long Imagenes { get; set; }

    [Required]
    public EstadoEnum Estado { get; set; }

    [Range(0, double.MaxValue)]
    public double PrecioPorDia { get; set; }

    public DateTime UltimaRevision { get; set; }

    public bool Activo { get; set; }
}