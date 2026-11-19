using APIAlquilerVehiculos.Models.Usuarios;

namespace APIAlquilerVehiculos.Repositories.Trabajadores;

public interface ITrabajadorRepository : ICrudRepository<Trabajador, long> {
    
    Task<(List<Trabajador> Items, int Total)> GetAllPagedAsync(
        int page, int pageSize, bool incluirInactivos = false, TipoTrabajador? tipo = null);

    Task<Trabajador?> GetByIdUsuarioAsync(long idUsuario);
    
    Task<bool> ExistsByDniAsync(string dni);

    // consultas para el 409 del service
    Task<bool> EsResponsableAsync(long idTrabajador);
    Task<bool> TieneReservasVivasAsync(long idTrabajador);
}