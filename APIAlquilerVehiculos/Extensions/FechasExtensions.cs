/// <summary>
/// Permite comprobar la diferencia horaria entre dos fechas agregando
/// horas extras a la fecha de recoigida para poder validar
/// </summary>
public static class FechasExtensios
{
    private static bool FechasValidas(
        this DateTime fechaRecogida,
        DateTime fechaDevolucion,
        int diferenciaEnHoras)
    {
        return fechaDevolucion >= fechaRecogida.AddHours(diferenciaEnHoras);
    }
}