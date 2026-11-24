using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIAlquilerVehiculos.Repositories;

public interface ICrudRepository<T,TKey>
{
    /// <summary>
    /// Obtiene todos los objetos de la base de datos.
    /// </summary>
    /// <returns> lista de los objetos guardados, activos</returns>
    public Task<List<T>> GetAllAsync( CancellationToken ct = default);

    /// <summary>
    /// Obtiene un objeto por su identificador único.
    /// </summary>
    /// <param name="id">Identificador del objeto a buscar</param>
    /// <returns> Devuelve el objeto si se encuentra, de lo contrario null</returns>
    public Task<T?> GetByIdAsync(TKey id,  CancellationToken ct = default);
    
    /// <summary>
    /// Crea un nuevo objeto en la base de datos.
    /// </summary>
    /// <param name="vehiculo">El objeto a crear</param>
    /// <returns> Devuelve el objeto creado si es exitoso, de lo contrario null</returns>
    public Task<T> CreateAsync(T obj,  CancellationToken ct = default);
    
    /// <summary>
    /// Actualiza un objeto existente en la base de datos.
    /// </summary>
    /// <param name="vehiculo">El objeto a actualizar</param>
    /// <returns> Devuelve el objeto actualizado si es exitoso, de lo contrario null</returns>
    public Task<T?> UpdateAsync(T obj,  CancellationToken ct = default );
    
    /// <summary>
    /// Elimina un objeto de la base de datos.
    /// </summary>
    /// <param name="id">Identificador del objeto a eliminar</param>
    /// <returns> Devuelve true si el objeto es eliminado, de lo contrario false</returns>
    public Task<bool> DeleteAsync(TKey id,  CancellationToken ct = default);
}