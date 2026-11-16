namespace APIAlquilerVehiculos.Models.Reservas;

/// <summary>
/// Cabecera reserva, relación en base 
/// lineas reserva, en la que una reserva
/// tiene muchas lineas de reservas.
/// Precio total se calcula en base a las lineas
/// </summary>
public class Reservas
{
    long Id {get; set;}
    string CodigoRefencia {get; set;} = string.Empty;
    long ClienteId {get; set;}
    // Relación con categoria por su id
    // en caso de borrado no se hace en cascada
    long CategoriaId {get; set;}
    long VehiculoId {get; set;}
    DateTime FechaCreacion {get; set;}
    DateTime FechaRecogida {get; set;}
    DateTime FechaDevolucion {get; set;}
    Estado Estado {get; set;}
    List<LineaReserva>? Lineas {get; set;}
    double PrecioTotal {get; set;}
    long PagoId {get; set;}
    string? Notas {get; set;}
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