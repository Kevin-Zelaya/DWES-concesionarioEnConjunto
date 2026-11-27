public enum TipoIncidencia
{
    DevolucionTardia,
    VehiculoNoDisponible
}

public record IncidenciaReserva(
    long ReservaId,
    string CodigoReferencia,
    long ClienteId,
    string VehiculoId,
    TipoIncidencia Tipo,
    string Descripcion,
    DateTime FechaDeteccion = default,
    DateTime? FechaUltimaNotificacion = null
);