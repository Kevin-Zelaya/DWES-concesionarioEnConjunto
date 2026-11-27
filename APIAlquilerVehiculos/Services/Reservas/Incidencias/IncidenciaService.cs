
/// <summary>
/// Gestiona las incidencias de las reservas mediante un diccionario en memoria.
/// Garantiza el acceso concurrente mediante un semáforo.
/// </summary>
public class IncidenciaService()
{
    /// <summary>
    /// Controla el acceso concurrente al diccionario de incidencias.
    /// </summary>
    private readonly SemaphoreSlim _semaforo = new(1, 1);

    /// <summary>
    /// Almacena las incidencias registradas, utilizando como clave
    /// el identificador de reserva y el tipo de incidencia.
    /// </summary>
    private readonly Dictionary<string, IncidenciaReserva> incidencias = new();

    /// <summary>
    /// Registra una incidencia, evitando duplicados.
    /// </summary>
    /// <param name="incidencia">Incidencia que se desea registrar.</param>
    public void Add(IncidenciaReserva incidencia)
    {
        string clave = $"{incidencia.ReservaId}_{incidencia.Tipo}";

        _semaforo.Wait();

        try
        {
            if (incidencias.ContainsKey(clave))
                return;

            incidencias.Add(clave, incidencia);
        }
        finally
        {
            _semaforo.Release();
        }

        Console.WriteLine("=== INCIDENCIA REGISTRADA ===");
        Console.WriteLine($"Reserva: {incidencia.CodigoReferencia}");
        Console.WriteLine($"Vehículo: {incidencia.VehiculoId}");
        Console.WriteLine($"Tipo: {incidencia.Tipo}");
        Console.WriteLine($"Descripción: {incidencia.Descripcion}");
        Console.WriteLine($"Fecha: {incidencia.FechaDeteccion}");
        Console.WriteLine($"Notificado: {incidencia.FechaUltimaNotificacion}");
        Console.WriteLine("=============================");
    }

    /// <summary>
    /// Obtiene una incidencia mediante el identificador de reserva y su tipo.
    /// </summary>
    /// <param name="reservaId">Identificador de la reserva.</param>
    /// <param name="tipo">Tipo de incidencia.</param>
    /// <returns>La incidencia encontrada o null si no existe.</returns>
    public IncidenciaReserva? Get(long reservaId, TipoIncidencia tipo)
    {
        string clave = $"{reservaId}_{tipo}";

        _semaforo.Wait();

        try
        {
            return incidencias.TryGetValue(clave, out IncidenciaReserva? incidencia)
                ? incidencia
                : null;
        }
        finally
        {
            _semaforo.Release();
        }
    }

    /// <summary>
    /// Elimina una incidencia del diccionario.
    /// </summary>
    /// <param name="reservaId">Identificador de la reserva.</param>
    /// <param name="tipo">Tipo de incidencia.</param>
    public void Delete(long reservaId, TipoIncidencia tipo)
    {
        string clave = $"{reservaId}_{tipo}";

        _semaforo.Wait();

        try
        {
            incidencias.Remove(clave);
        }
        finally
        {
            _semaforo.Release();
        }
    }

    /// <summary>
    /// Obtiene todas las incidencias registradas.
    /// </summary>
    /// <returns>Copia del diccionario con las incidencias almacenadas.</returns>
    public Dictionary<string, IncidenciaReserva> GetAll()
    {
        _semaforo.Wait();

        try
        {
            return new Dictionary<string, IncidenciaReserva>(incidencias);
        }
        finally
        {
            _semaforo.Release();
        }
    }

    /// <summary>
    /// Actualiza una incidencia existente en el diccionario.
    /// </summary>
    /// <param name="clave">Clave identificadora de la incidencia.</param>
    /// <param name="incidencia">Incidencia con los datos actualizados.</param>
    public void Update(string clave, IncidenciaReserva incidencia)
    {
        _semaforo.Wait();

        try
        {
            if (!incidencias.ContainsKey(clave))
                return;

            incidencias[clave] = incidencia;
        }
        finally
        {
            _semaforo.Release();
        }
    }
    /// <summary>
    /// Elimina todas las incidencias asociadas a una reserva.
    /// </summary>
    /// <param name="reservaId">Identificador de la reserva.</param>
    public void DeleteAll(long reservaId)
    {
        _semaforo.Wait();

        try
        {
            var claves = incidencias
                .Where(i => i.Value.ReservaId == reservaId)
                .Select(i => i.Key)
                .ToList();

            foreach (var clave in claves)
                incidencias.Remove(clave);
        }
        finally
        {
            _semaforo.Release();
        }
    }
}
