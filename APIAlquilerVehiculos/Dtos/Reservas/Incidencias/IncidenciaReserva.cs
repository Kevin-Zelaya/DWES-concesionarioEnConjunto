public enum TipoIncidencia
{
    DevolucionTardia,
    VehiculoNoDisponible
}

public record IncidenciaReserva(
    long ReservaId,
    string CodigoReferencia,
    string VehiculoId,
    TipoIncidencia Tipo,
    string Descripcion,
    DateTime FechaDeteccion
);