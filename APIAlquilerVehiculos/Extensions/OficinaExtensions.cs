using APIAlquilerVehiculos.Models;

namespace APIAlquilerVehiculos.Extensions;

public static class OficinaExtensions
{
    /// <summary>
    /// Comprueba si una fecha y hora están dentro del horario de atención de la oficina.
    /// La oficina permanece cerrada los sábados y domingos.
    /// </summary>
    /// <param name="oficina">Oficina cuyo horario se desea comprobar.</param>
    /// <param name="fecha">Fecha y hora que se desea validar.</param>
    /// <returns>True si la oficina está abierta; false en caso contrario.</returns>
    public static bool EstaDentroDelHorario(this Oficina oficina, DateTime fecha)
    {
        // Comprobamos si es fin de semana
        if (fecha.DayOfWeek == DayOfWeek.Saturday ||
            fecha.DayOfWeek == DayOfWeek.Sunday)
            return false;

        // Separamos la hora de apertura y la de cierre
        var horas = oficina.Horario.Split('-');

        if (horas.Length != 2)
            return false;

        // Convertimos las horas a TimeOnly
        if (!TimeOnly.TryParse(horas[0], out var apertura) ||
            !TimeOnly.TryParse(horas[1], out var cierre))
            return false;

        // Obtenemos la hora de la fecha que queremos comprobar
        var hora = TimeOnly.FromDateTime(fecha);

        // Comprobamos si está dentro del horario
        return hora >= apertura && hora <= cierre;
    }
}