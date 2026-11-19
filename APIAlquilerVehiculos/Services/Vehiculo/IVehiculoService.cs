using APIAlquilerVehiculos.Dtos.Vehiculo;
using APIAlquilerVehiculos.Services.Commons;

namespace APIAlquilerVehiculos.Services.Vehiculo;

public interface IVehiculoService : IService<VehiculoCreateDto,VehiculoUpdateDto,VehiculoResponseDto,string>{}