
using APIAlquilerVehiculos.Models.Reservas;

/// <summary>
/// Representa un intento de pago asociado a una reserva.
/// </summary>
public class Pago
{
    /// <summary>
    /// Identificador único del pago.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Identificador de la reserva asociada.
    /// </summary>
    public long ReservaId { get; set; }

    /// <summary>
    /// Reserva a la que pertenece el pago.
    /// </summary>
    public Reserva Reserva { get; set; } = null!;

    /// <summary>
    /// Cantidad total que se ha intentado cobrar o se ha cobrado.
    /// </summary>
    public decimal Importe { get; set; }

    /// <summary>
    /// Divisa utilizada en el pago. Por defecto, euros.
    /// </summary>
    public string Divisa { get; set; } = "EUR";

    /// <summary>
    /// Estado actual del pago.
    /// </summary>
    public EstadoPago Estado { get; set; }

    /// <summary>
    /// Marca de la tarjeta utilizada para realizar el pago.
    /// </summary>
    public MarcaTarjeta MarcaTarjeta { get; set; }

    /// <summary>
    /// Fecha en la que se realizó el intento de pago.
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Últimos cuatro dígitos de la tarjeta.
    /// </summary>
    public string UltimosCuatroDigitos { get; set; } = string.Empty;

    /// <summary>
    /// Número de tarjeta cifrado.
    /// Nunca se almacena en texto plano.
    /// </summary>
    public string TarjetaCifrada { get; set; } = string.Empty;

    /// <summary>
    /// Referencia devuelta por la pasarela de pago.
    /// </summary>
    public string ReferenciaPasarela { get; set; } = string.Empty;
}


// Estados posibles de un pago.
public enum EstadoPago
{
    // El pago se ha realizado correctamente.
    Aprobado,
    // El pago ha sido rechazado por la pasarela o entidad bancaria.
    Rechazado,
    // El importe del pago ha sido devuelto al cliente.
    Reembolsado
}

// Marcas de tarjeta admitidas.
public enum MarcaTarjeta
{
    // Tarjeta Visa.
    Visa,
    // Tarjeta Mastercard.
    Mastercard,
    // Cualquier otra marca de tarjeta.
    Otra
}