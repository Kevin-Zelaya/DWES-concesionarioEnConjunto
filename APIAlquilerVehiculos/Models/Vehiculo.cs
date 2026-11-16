using APIAlquilerVehiculos.Models.Enums;

namespace APIAlquilerVehiculos.Models;

/// <summary>
/// Representa un vehículo en el sistema de alquiler de vehículos.
/// </summary>
public class Vehiculo
{
    /// <summary>
    /// Identificador único del vehículo.
    /// El cual es un sting de 11 caracteres codificado igual que el sistema de YouTuve.
    /// Este se genera en el servidor.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Matrícula del vehículo.
    /// </summary>
    public string Matricula { get; set; } = string.Empty;

    /// <summary>
    /// Identificador de la marca del vehículo.
    /// </summary>
    public int Marca_id { get; set; }

    /// <summary>
    /// Establece el identificador del modelo del vehículo.
    /// </summary>
    public int Modelo_id { get; set; }

    /// <summary>
    /// Establece el código Acriss del vehículo.
    /// </summary>
    public string Acriss { get; set; } = string.Empty;
    
    /// <summary>
    /// Establece la categoría del vehículo.
    /// </summary>
    public CategoriaEnum categoria { get; set; }
    
    /// <summary>
    /// Establece el identificador del concesionario al que pertenece el vehículo.
    /// </summary>
    public int Concesionario_id { get; set; }
    
    /// <summary>
    /// Color del vehículo.
    /// </summary>
    public string Color { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo de combustible del vehículo.
    /// </summary>
    public string Combustible { get; set; } = string.Empty;
    
    /// <summary>
    /// Capacidad del maletero del vehículo en litros.
    /// </summary>
    public double Maletero { get; set; }
    
    /// <summary>
    /// Número de plazas del vehículo.
    /// </summary>
    public int NumeroPlazas { get; set; }
    
    /// <summary>
    /// Autonomía del vehículo en kilómetros.
    /// </summary>
    public double Autonomia { get; set; }
    
    /// <summary>
    /// Lista de imágenes asociadas al vehículo.
    /// </summary>
    public int Imagenes { get; set; }
    
    /// <summary>
    /// Estado actual del vehículo.
    /// </summary>
    public EstadoEnum Estado { get; set; }
    
    /// <summary>
    /// Fecha de la última revisión del vehículo.
    /// </summary>
    public DateTime UltimaRevision { get; set; }
    
    /// <summary>
    /// Indica si el vehiculo está activo o no.
    /// </summary>
    public bool Activo { get; set; }
    
    /// <summary>
    /// Fecha de creación del vehiculo.
    /// </summary>
    public DateTime? CreatedAt { get; set; }
    
    /// <summary>
    /// Fecha de última actualización del vehiculo.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Fecha de eliminación del vehiculo (si ha sido eliminado).
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}