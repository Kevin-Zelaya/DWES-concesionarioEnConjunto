using APIAlquilerVehiculos.Models.Enums;

namespace APIAlquilerVehiculos.Models.Reservas;

/// <summary>
/// Cabecera reserva, relación en base 
/// lineas reserva, en la que una reserva
/// tiene muchas lineas de reservas.
/// Precio total se calcula en base a las lineas
/// </summary>
public class Reserva
{
    public long Id {get; set;}
    public string CodigoRefencia {get; set;} = string.Empty;
    public long ClienteId {get; set;}
    // Trabajador que lo crea
    public long TrabajadorId {get; set;}
    public CategoriaEnum Categoria {get; set;}
    public long VehiculoId {get; set;}
    public DateTime FechaCreacion {get; set;}
    public DateTime FechaRecogida {get; set;}
    public DateTime FechaDevolucion {get; set;}
    public Estado Estado {get; set;}
    public List<LineaReserva>? Lineas {get; set;}
    public double PrecioTotal {get; set;}
    public long PagoId {get; set;}
    public string? Notas {get; set;}
}

/// <summary>
/// Posibles estados de una reserva
/// enum de posibilidades.
/// </summary>
public enum Estado
{
    Pendiente,
    Confirmada,
    EnCurso,
    Finalizada,
    Caducada,
    Cancelada,
    NoPresendato
}