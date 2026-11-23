using APIAlquilerVehiculos.Models.Reservas;

public static class EstadoReservaExtensions
{
    public static bool PuedeTransicionarA(
        this Estado estadoActual,
        Estado nuevoEstado)
    {
        return estadoActual switch
        {
            Estado.Pendiente =>
                nuevoEstado == Estado.Confirmada ||
                nuevoEstado == Estado.Cancelada ||
                nuevoEstado == Estado.Caducada,

            Estado.Confirmada =>
                nuevoEstado == Estado.Cancelada ||
                nuevoEstado == Estado.EnCurso ||
                nuevoEstado == Estado.NoPresentado,

            Estado.EnCurso =>
                nuevoEstado == Estado.Finalizada,

            Estado.Finalizada or
            Estado.Caducada or
            Estado.Cancelada or
            Estado.NoPresentado => false,

            _ => false
        };
    }
}