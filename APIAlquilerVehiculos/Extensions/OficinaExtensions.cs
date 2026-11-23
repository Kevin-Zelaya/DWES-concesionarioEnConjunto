
using APIAlquilerVehiculos.Models;

namespace APIAlquilerVehiculos.Extensions;

public static class OficinaExtensions
{
    /// <summary>
    /// Comprueba si una fecha y hora se encuentran dentro del horario
    /// de atención de la oficina.
    /// La oficina permanece cerrada los sábados y domingos.
    /// </summary>
    /// <param name="oficina">Oficina cuyo horario se desea comprobar.</param>
    /// <param name="fecha">Fecha y hora que se desea validar.</param>
    /// <returns>
    /// true si la oficina está abierta en ese momento;
    /// false en caso contrario.
    /// </returns>
    public static bool EstaDentroDelHorario(
        this Oficina oficina,
        DateTime fecha)
    {
        // La oficina abre de lunes a viernes.
        if (fecha.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return false;
        }

        // Separar la hora de apertura y cierre.
        var horario = oficina.Horario.Split('-');

        if (horario.Length != 2)
        {
            return false;
        }

        // Validar y convertir las horas.
        if (!TimeOnly.TryParseExact(
            horario[0],
            "HH:mm",
            out var horaApertura) ||
            !TimeOnly.TryParseExact(
            horario[1],
            "HH:mm",
            out var horaCierre))
        {
            return false;
        }

        var horaOperacion = TimeOnly.FromDateTime(fecha);

        return horaOperacion >= horaApertura &&
               horaOperacion <= horaCierre;
    }
}
