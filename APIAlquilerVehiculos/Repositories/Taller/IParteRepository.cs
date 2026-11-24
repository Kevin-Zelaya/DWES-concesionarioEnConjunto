using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Models.Usuarios;

namespace APIAlquilerVehiculos.Repositories.Taller;

public interface IParteRepository : ICrudRepository<Parte, long>
{
    Task<bool> TieneParteAbiertoAsync(string vehiculoId, CancellationToken ct = default);
    Task<Trabajador?> GetTrabajadorByIdAsync(long trabajadorId, CancellationToken ct = default);
    Task<Vehiculo?> GetVehiculoByIdAsync(string vehiculoId, CancellationToken ct = default);
    Task<List<Reserva>> GetReservasVivasByVehiculoIdAsync(string vehiculoId, CancellationToken ct = default);
    Task UpdateVehiculoAsync(Vehiculo vehiculo, CancellationToken ct = default);
}