using System.Text.Json.Serialization;
using APIAlquilerVehiculos.Models.Reservas;

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
   [JsonIgnore]
   public Reserva Reserva { get; set; } = null!;
   
   /// <summary> Referencia al nombre pero no con fk </summary>
   public TipoLinea Tipo {get; set;} 
   public string Concepto {get; set;} = string.Empty;

   public int Cantidad {get; set;}
   public decimal PrecioUnitario {get; set;}
   public TipoPrecio TipoPrecio {get; set;}
   public decimal Subtotal {get; set;}
   // Evita el delegar este calculo al servicio
   public void CalcularSubtotal()
    {
        Subtotal = TipoPrecio switch
        {
            TipoPrecio.Diario => PrecioUnitario * Cantidad,
            TipoPrecio.Unico => PrecioUnitario * Cantidad,
            _ => 0m
        };
    }
}


public enum TipoLinea
{
    Vehiculo,
    Poliza,
    Extra,
    Recargo
}


public enum TipoPrecio
{
    Diario,
    Unico
}