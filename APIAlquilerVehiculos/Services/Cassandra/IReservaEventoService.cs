using APIAlquilerVehiculos.Entities.Cassandra;

namespace APIAlquilerVehiculos.Services.Cassandra;

/// <summary>
/// Define las operaciones para gestionar los eventos de reservas
/// almacenados en Cassandra.
/// </summary>
public interface IReservaEventoService
{
    Task RegistrarEventoAsync(ReservaEvento evento);
}