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
   public long Id {get; set;}
   /// <summary> fk de reserva, así para poder agregar mismo
   /// concepto pero con diferente precio por día </summary>
   public long ReservaId{get; set;}
   /// <summary> Referencia al nombre pero no con fk </summary>
   public string Tipo {get; set;} = string.Empty;
   public string Concepto {get; set;} = string.Empty;
   public int Cantidad {get; set;}
   public double PrecioUnitario {get; set;}
   public double Subtotal {get; set;}
}