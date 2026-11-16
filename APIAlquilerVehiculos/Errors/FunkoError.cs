public static class FunkoError
{
    public static NotFoundError NotFound(string id) => NotFoundError.FromResource("Vehiculo", id);
    public static ConflictError NombreDuplicado(string matricula) => ConflictError.Duplicate("Vehículo", matricula);
}