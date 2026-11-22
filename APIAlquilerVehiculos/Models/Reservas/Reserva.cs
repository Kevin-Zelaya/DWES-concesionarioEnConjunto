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
    public string? CodigoRefencia {get; set;}
    public long ClienteId {get; set;}
    // Trabajador que lo crea
    public long? TrabajadorId {get; set;}
    public CategoriaEnum Categoria {get; set;}
    public string VehiculoId {get; set;}
    public Vehiculo? Vehiculo {get; set;}
    public DateTime FechaCreacion {get; set;}
    public DateTime FechaRecogida {get; set;}
    public DateTime FechaDevolucion {get; set;}
    public Estado Estado {get; set;}
    public List<LineaReserva> Lineas {get; set;} = [];
    public decimal PrecioTotal { get; set; }
    public long? PagoId {get; set;}
    public Pago? Pago {get; set;}
    public string? Notas {get; set;}
    // control de concurrencia
    public uint RowVersion {get; set;}
    public int CalcularDiasAlquiler()
    {
        return (FechaDevolucion.Date - FechaRecogida.Date).Days;
    }
    public static int CalcularDiasAlquiler(
        DateTime FechaRecogida,
        DateTime FechaDevolución
    )
    {
        return (FechaRecogida.Date - FechaDevolución.Date).Days;
    }
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
    NoPresentado
}