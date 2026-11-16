/// <summary>
/// Caracteristicas de una linea dentro de las reservas
/// relacion de uno a muchos en el que una reserva tiene muchas lineas
/// pero muchas lineas pertenecen a una reserva.
/// Relación fk con ReservaId
/// 
/// El tipo es relación por nombre no tiene fk
/// por que se puede relacionar a dos tablas diferentes
/// </summary>
public class LineaReserva
{
    long Id {get; set;}
    /// <summary> fk de reserva, así para poder agregar mismo
    /// concepto pero con diferente precio por día </summary>
    long ReservaId{get; set;}
    /// <summary> Referencia al nombre pero no con fk </summary>
    string Tipo {get; set;} = string.Empty;
    string Concepto {get; set;} = string.Empty;
    int Cantidad {get; set;}
    double PrecioUnitario {get; set;}
    double Subtotal {get; set;}
}