using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Models.Usuarios;

namespace APIAlquilerVehiculos.Repositories.Taller;

public interface IParteRepository : ICrudRepository<Parte, long>
{
    Task<bool> TieneParteAbiertoAsync(string vehiculoId);
    Task<Trabajador?> GetTrabajadorByIdAsync(long trabajadorId);
    Task<Vehiculo?> GetVehiculoByIdAsync(string vehiculoId);
    Task<List<Reserva>> GetReservasVivasByVehiculoIdAsync(string vehiculoId);
    Task UpdateVehiculoAsync(Vehiculo vehiculo);
}