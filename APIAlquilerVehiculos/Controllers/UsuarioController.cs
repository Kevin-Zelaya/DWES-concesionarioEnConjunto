using APIAlquilerVehiculos.Dtos.Usuarios;
using APIAlquilerVehiculos.Services.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;

namespace APIAlquilerVehiculos.Controllers;


/// <summary>
/// Controlador encargado de observar a todos los usuarios.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class UsuarioController(IUsuarioService service) : ControllerBase
{
    /// <summary>
    /// Servicio encargado de gestionar la lógica de negocio de los usuarios.
    /// </summary>
    private readonly IUsuarioService _service = service;
    
    /// <summary>
    /// Obtiene los usuarios de forma paginada.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RequireAdminRole")]
    [SwaggerOperation(
        Summary = "Obtener todos los usuarios",
        Description = "Devuelve una lista paginada de usuarios.",
        OperationId = "GetUsuariosPaginadosAsync",
        Tags = new[] { "Usuarios" }
    )]
    [SwaggerResponse(200, "Lista de usuarios obtenida correctamente", typeof(PaginatedResouce<UsuarioDto>))]
    [SwaggerResponse(400, "Parámetros de búsqueda inválidos")]
    [SwaggerResponse(401, "No autenticado")]
    [SwaggerResponse(403, "Permisos insuficientes")]
    [SwaggerResponse(500, "Error interno del servidor")]
    public async Task<ActionResult<PaginatedResouce<UsuarioDto>>> GetAllAsync(CancellationToken cancellationToken, 
        [FromQuery, SwaggerParameter("Número de pagina", Required = true)] int pageNumber = 1,
        [FromQuery, SwaggerParameter("Número de elementos por pagina", Required = true)] int pageSize = 5)
    {
        Log.Information("PageNumber: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);

        if (pageNumber < 1 || pageSize < 1)
            return BadRequest("La página y el tamaño deben ser mayores que 0.");
        
        var resultado = await _service.GetPaginatedUsuariosAsync(pageNumber, pageSize, cancellationToken);

        return resultado.IsSuccess
            ? Ok(resultado.Value)
            : resultado.Error.ToHttpResult<PaginatedResouce<UsuarioDto>>();
    }

    /// <summary>
    /// Obtiene un usuario especifico por su identificador.
    /// </summary>
    [HttpGet("{id:long}")]
    [Authorize(Policy = "RequireAdminRole")]
    [SwaggerOperation(
        Summary = "Obtener usuario por ID",
        Description = "Devuelve el detalle de un usuario a partir de su identificador.",
        OperationId = "GetUsuarioByIdAsync",
        Tags = new[] { "Usuarios" }
    )]
    [SwaggerResponse(200, "Detalle del usuario obtenido correctamente", typeof(UsuarioDetalleDto))]
    [SwaggerResponse(401, "No autenticado")]
    [SwaggerResponse(403, "Permisos insuficientes")]
    [SwaggerResponse(404, "Usuario no encontrado")]
    [SwaggerResponse(500, "Error interno del servidor")]
    public async Task<ActionResult<UsuarioDetalleDto>> GetByIdAsync([FromRoute, SwaggerRequestBody("Identificador del usuario", Required = true)] long id,
        CancellationToken cancellationToken)
    {
        var resultado = await _service.GetByIdAsync(id, cancellationToken);

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error);

        return Ok(resultado.Value);
    }

}