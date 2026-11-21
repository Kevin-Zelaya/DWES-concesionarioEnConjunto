using APIAlquilerVehiculos.Errors.Common;

public static class DisponibilidadErrors
{
    public static NotFoundError VehiculoNotFound(string identificador) =>
        NotFoundError.FromResource("Vehículo", identificador);

    public static ConflictError VehiculoNoDisponible(string identificador) =>
        ConflictError.Business(
            $"El vehículo '{identificador}' no está disponible para las fechas solicitadas.");

    public static BusinessRuleError FechasNoValidas() =>
        new("La fecha de recogida debe ser anterior a la fecha de devolución.");
}