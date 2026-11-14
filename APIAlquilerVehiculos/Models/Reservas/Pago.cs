// Representa el pago asociado a una reserva.
public class Pago
{
    // Identificador único del pago.
    public long Id { get; set; }
    // Cantidad total que se ha intentado cobrar o se ha cobrado.
    public double Importe { get; set; }
    // Divisa utilizada en el pago. Por defecto, euros.
    public string Divisa { get; set; } = "EUR";
    // Estado actual del pago: Aprobado, Rechazado o Reembolsado.
    public EstadoPago Estado { get; set; }
    // Marca de la tarjeta utilizada para realizar el pago.
    public MarcaTarjeta MarcaTarjeta { get; set; }
    // Últimos dígitos de la tarjeta, para poder identificarla sin mostrarla completa.
    public string? UltimosDigito { get; set; }
    // Número de tarjeta cifrado. Nunca se debe almacenar el número en texto plano.
    public string? TarjetaCifrada { get; set; }
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