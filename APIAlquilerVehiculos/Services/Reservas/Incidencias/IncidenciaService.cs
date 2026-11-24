using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Gestiona las incidencias de las reservas mediante la caché en memoria.
/// </summary>
/// <param name="cache">Servicio de caché en memoria.</param>
public class IncidenciaService(IMemoryCache cache)
{
    /// <summary>
    /// Registra una incidencia en caché durante 7 días, evitando duplicados.
    /// </summary>
    /// <param name="incidencia">Incidencia que se desea registrar.</param>
    public void Registrar(IncidenciaReserva incidencia)
    {
        string clave = $"{incidencia.ReservaId}_{incidencia.Tipo}";

        if (cache.TryGetValue(clave, out _))
            return;

        cache.Set(clave, incidencia, TimeSpan.FromDays(7));

        Console.WriteLine("=== INCIDENCIA REGISTRADA ===");
        Console.WriteLine($"Reserva: {incidencia.CodigoReferencia}");
        Console.WriteLine($"Vehículo: {incidencia.VehiculoId}");
        Console.WriteLine($"Tipo: {incidencia.Tipo}");
        Console.WriteLine($"Descripción: {incidencia.Descripcion}");
        Console.WriteLine($"Fecha: {incidencia.FechaDeteccion}");
        Console.WriteLine("=============================");
    }

    /// <summary>
    /// Obtiene una incidencia mediante el identificador de reserva y su tipo.
    /// </summary>
    /// <param name="reservaId">Identificador de la reserva.</param>
    /// <param name="tipo">Tipo de incidencia.</param>
    /// <returns>La incidencia encontrada o null si no existe.</returns>
    public IncidenciaReserva? Obtener(long reservaId, TipoIncidencia tipo)
    {
        string clave = $"{reservaId}_{tipo}";

        return cache.Get<IncidenciaReserva>(clave);
    }

    /// <summary>
    /// Elimina una incidencia de la caché.
    /// </summary>
    /// <param name="reservaId">Identificador de la reserva.</param>
    /// <param name="tipo">Tipo de incidencia.</param>
    public void Eliminar(long reservaId, TipoIncidencia tipo)
    {
        string clave = $"{reservaId}_{tipo}";

        cache.Remove(clave);
    }
}