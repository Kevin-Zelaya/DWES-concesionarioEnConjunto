using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Models.Usuarios;

namespace APIAlquilerVehiculos.Repositories.Taller;

public class ParteRepository : IParteRepository
{
    private readonly List<Parte> _partes = [];
    private long _nextId = 1;

    // Datos de prueba en memoria para desarrollar 
    private readonly List<Trabajador> _trabajadores =
    [
        new Trabajador
        {
            Id = 1,
            Nombre = "Carlos",
            Apellidos = "Gómez",
            IsActivo = true,
            Dni = null,
            TipoTrabajador = TipoTrabajador.Operario
        }
    ];

    private readonly List<Vehiculo> _vehiculos =
    [
        new Vehiculo { Id = "veh00000001", Matricula = "1234BCD", Estado = EstadoEnum.Disponible, Activo = true }
    ];

    public Task<List<Parte>> GetAllAsync() =>
        Task.FromResult(_partes.ToList());

    public Task<Parte?> GetByIdAsync(long id) =>
        Task.FromResult(_partes.FirstOrDefault(p => p.Id == id));

    public Task<Parte> CreateAsync(Parte obj)
    {
        obj.Id = _nextId++;
        _partes.Add(obj);
        return Task.FromResult(obj);
    }

    public Task<Parte?> UpdateAsync(Parte obj)
    {
        var index = _partes.FindIndex(p => p.Id == obj.Id);
        if (index == -1) return Task.FromResult<Parte?>(null);

        _partes[index] = obj;
        return Task.FromResult<Parte?>(obj);
    }

    public Task<bool> DeleteAsync(long id)
    {
        var parte = _partes.FirstOrDefault(p => p.Id == id);
        if (parte is null) return Task.FromResult(false);

        return Task.FromResult(_partes.Remove(parte));
    }

    public Task<bool> TieneParteAbiertoAsync(string vehiculoId) =>
        Task.FromResult(_partes.Any(p => p.VehiculoId == vehiculoId && p.Estado == EstadoParte.Abierto));

    public Task<Trabajador?> GetTrabajadorByIdAsync(long trabajadorId) =>
        Task.FromResult(_trabajadores.FirstOrDefault(t => t.Id == trabajadorId));

    public Task<Vehiculo?> GetVehiculoByIdAsync(string vehiculoId) =>
        Task.FromResult(_vehiculos.FirstOrDefault(v => v.Id == vehiculoId));

    public Task<List<Reserva>> GetReservasVivasByVehiculoIdAsync(string vehiculoId) =>
        Task.FromResult(new List<Reserva>());

    public Task UpdateVehiculoAsync(Vehiculo vehiculo) =>
        Task.CompletedTask;
}