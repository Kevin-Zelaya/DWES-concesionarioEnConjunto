namespace APIAlquilerVehiculos.Models;

public class Oficina
{
    public long Id { get; init; }
    
    public string Name { get; init; } = string.Empty;
    
    public Direccion Direccion { get; set; } = null!;
    
    public string Telefono { get; init; } = string.Empty;
    
    public Localizacion Localizacion { get; set; } = null!;
    
    public string Horario { get; init; } = string.Empty;
    
    public long IdResponsable { get; init; } // Id del trabajador responsable
    
    public Trabajador Responsable { get; set; } = null!; // acceder a Trabajador
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}

// OwnsOne
public class Direccion
{
    public string Calle { get; init; } = string.Empty;
    public string CodigoPostal { get; init; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
}

public class Localizacion
{
    public decimal Latitud { get; set; }

    public decimal Longitud { get; set; }
}