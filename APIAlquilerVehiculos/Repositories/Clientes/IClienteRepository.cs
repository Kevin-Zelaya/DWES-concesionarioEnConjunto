using APIAlquilerVehiculos.Dtos.Cientes;
using APIAlquilerVehiculos.Models.Usuarios;

namespace APIAlquilerVehiculos.Repositories.Clientes;

public interface IClienteRepository
{
    Task<(List<Cliente> Items, int TotalCount)> FindAllPagedAsync(ClienteFilterDto dto);
    Task<Cliente?> FindByIdAsync(long id);
    Task<Cliente?> FindByUserId(long id);

    Task<Cliente?> UpdateAsync(long id, Cliente updatedCliente);
    Task<bool> DeleteAsync(long id);

    //Task<bool> ExistsByDocumentAsync(string name, long? id = null);

    Task<bool> ExistsClientesWithIdUserAsync(long userId);
}