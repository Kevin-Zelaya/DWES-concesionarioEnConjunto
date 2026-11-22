using APIAlquilerVehiculos.Dtos.Cientes;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Services.Clientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace APIAlquilerVehiculos.Controllers;

/// <summary>
/// Controlador encargado de gestionar las operaciones de los clientes.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ClienteController(IClienteService service) : ControllerBase
{
     /// <summary>
    /// Servicio encargado de gestionar la lógica de negocio de los clientes.
    /// </summary>
    private readonly IClienteService _service = service;

    /// <summary>
    /// Obtiene los clientes de forma paginada y con posibles filtros.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RequireAdmin")]
    [SwaggerOperation(
        Summary = "Obtener todos los clientes",
        Description = "Devuelve una lista paginada de clientes con filtros opcionales",
        OperationId = "GetAllAsync",
        Tags = new[] { "Clientes" }
    )]
    //[SwaggerRequestExample(typeof(FunkoFilterDto), typeof(FunkoFilterExample))] 
    [SwaggerResponse(200, "Lista de clientes obtenida correctamente", typeof(List<ClienteDto>))]
    [SwaggerResponse(400, "Parametros invalidos")]
    [SwaggerResponse(404, "No se ha podido realizar la acción")]
    [SwaggerResponse(404, "No se ha podido realizar la acción")]
    public async Task<ActionResult<List<ClienteDto>>> GetAllAsync([FromQuery, SwaggerParameter("Filtros opcionales", Required = false)] ClienteFilterDto filter,
        CancellationToken cancellationToken)
    {
        var resultado = await _service.FindAllPagedAsync(filter, cancellationToken);

        // Si se produce un error, devolvemos su mensaje y código HTTP.
        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error);

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtiene un trabajador por su identificador.
    /// </summary>
    [HttpGet("{id:long}")]
    [Authorize(Roles = $"{Roles.Administrador},{Roles.Responsable}")]
    [SwaggerOperation(
        Summary = "Obtener todos los funkos",
        Description = "Devuelve una lista paginada de funkos con filtros opcionales",
        OperationId = "GetAllAsync",
        Tags = new[] { "Funkos" }
    )]
    //[SwaggerRequestExample(typeof(FunkoFilterDto), typeof(FunkoFilterExample))] 
    [SwaggerResponse(200, "Lista de funkos obtenida", typeof(List<ClienteDto>))]
    [SwaggerResponse(400, "Parametros invalidos")]
    [SwaggerResponse(404, "No se ha podido realizar la acción")]
    public async Task<ActionResult<ClienteDto>> GetByIdAsync([FromRoute, SwaggerRequestBody("Identificador del cliente", Required = true)] long id,
        CancellationToken cancellationToken)
    {
        var resultado = await _service.GetByIdAsync(id, cancellationToken);

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error);

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Actualiza los datos de un cliente existente.
    /// </summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = $"{Roles.Administrador}")]
    [SwaggerOperation(
        Summary = "Actualización de los datos de un cliente",
        Description = "Devuelve los datos del cliente y del usuario cliente actualizados",
        OperationId = "UpdateAsync",
        Tags = new[] { "Clientes" }
    )]
    //[SwaggerRequestExample(typeof(UpdateClienteDto), typeof(UpdateClienteExample))] 
    [SwaggerResponse(200, "Cliente actualizado correctamente", typeof(ClienteDto))]
    [SwaggerResponse(400, "Errores de validación")]
    [SwaggerResponse(404, "No se encontro el funko")]
    [SwaggerResponse(404, "No se guardo el funko")]
    [SwaggerResponse(409, "Ya existe un funko con ese nombre")]
    [SwaggerResponse(409, "Conflicto de concurrencia")]
    public async Task<ActionResult<ClienteDto>> UpdateAsync(
        [FromRoute, SwaggerParameter("Identificador del cliente", Required = true)] long id, 
        [FromForm, SwaggerRequestBody("Datos actualizados del cliente", Required = true)] UpdateClienteDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await _service.UpdateAsync(id, dto, cancellationToken);

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error);

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Da de baja lógicamente a un cliente.
    /// </summary>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = $"{Roles.Administrador}")]
    public async Task<IActionResult> DeleteAsync([FromRoute, SwaggerParameter("Identificador del cliente", Required = true)] long id, CancellationToken cancellationToken)
    {
        var resultado = await _service.DeleteAsync(id, cancellationToken);

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error);

        return NoContent();
    }
}