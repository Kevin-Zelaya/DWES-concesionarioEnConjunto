using APIAlquilerVehiculos.Models.Usuarios;

namespace APIAlquilerVehiculos.Extensions;

public static class ClienteExtensions
{
    /// <summary>
    /// Calcula la edad actual del cliente teniendo en cuenta su fecha de nacimiento.
    /// </summary>
    /// <param name="cliente">Cliente cuya edad se desea calcular.</param>
    /// <returns>Edad actual del cliente en años.</returns>
    public static int CalcularEdad(this Cliente cliente)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var nacimiento = DateOnly.FromDateTime(cliente.FechaNacimiento);

        // Calculamos la diferencia de años
        var edad = hoy.Year - nacimiento.Year;

        // Comprobamos si todavía no ha cumplido años este año
        if (hoy < nacimiento.AddYears(edad))
            edad--;

        return edad;
    }

    /// <summary>
    /// Comprueba si el cliente tiene al menos 18 años.
    /// </summary>
    /// <param name="cliente">Cliente cuya mayoría de edad se desea comprobar.</param>
    /// <returns>True si tiene 18 años o más; false en caso contrario.</returns>
    public static bool EsMayorDeEdad(this Cliente cliente)
    {
        return cliente.CalcularEdad() >= 18;
    }
}