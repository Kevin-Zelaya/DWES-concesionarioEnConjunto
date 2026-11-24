using APIAlquilerVehiculos.Dtos.Cientes;
using APIAlquilerVehiculos.Models.Usuarios;

namespace APIAlquilerVehiculos.Repositories.Clientes;

public interface IClienteRepository
{
    /// <summary>
    ///  Busqueda de clientes con paginado y filtros de busqueda
    /// </summary>
    /// <param name="page">Número de pagina</param>
    /// <param name="pageSize">Número de elementos por pagina</param>
    /// <param name="incluirInactivos">Incluir clientes inactivos</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns></returns>
    Task<(List<Cliente> Items, int Total)> FindAllPagedAsync(int page, int pageSize, bool incluirInactivos = false,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    ///  Buscar un cliente concreto con su identificador
    ///  Con acceso el Administrador y el propio cliente
    /// </summary>
    /// <param name="id">Identificador del cliente</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve los datos especificos del cliente</returns>
    Task<Cliente?> FindByIdAsync(long id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns></returns>
    Task<Cliente?> FindByUserId(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updatedCliente"></param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns></returns>
    Task<Cliente?> UpdateAsync(long id, Cliente updatedCliente, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualizar estado del cliente de baja a activo
    /// Acceso único por el Administrador
    /// </summary>
    /// <param name="id">identificador del cliente</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve el cliente renovado</returns>
    Task<Cliente?> UpdatePartialAsync(long id, CancellationToken cancellationToken = default);

    
    /// <summary>
    /// Dar de baja logica un cliente
    /// Y este no debe poder aparecer en niguna otra acción, ni debe poder chocar valores con otros usuarios
    /// </summary>
    /// <param name="id">Identificador del cliente</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve verdadero (true) si se ha eliminado</returns>
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    ///  Comprobar si ya existe un cliente con un perfil usuario
    /// </summary>
    /// <param name="id">Identificador del usuario</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona</param>
    /// <returns>Devuelve true si si existe</returns>
    Task<bool> ExistsClientesWithIdUserAsync(long id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Comprobar si el cliente tienen alguna reserva viva (Pendiente, Confirmada o EnCurso)
    /// </summary>
    /// <param name="idCliente">Identificador del cliente</param>
    /// <returns>Devuelve true si tiene reservas aún vivas</returns>
    Task<bool> TieneReservasVivasAsync(long idCliente);
}