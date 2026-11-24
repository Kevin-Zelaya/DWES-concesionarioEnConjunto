using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// Controlador encargado de gestionar los recargos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RecargoController(IRecargoService _service) : ControllerBase
{
    /// <summary>
    /// Obtiene todos los recargos activos.
    /// </summary>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Listado de recargos.</returns>
    [HttpGet]
    public async Task<ActionResult<List<RecargoResponseDto>>> GetAll(CancellationToken ct)
    {
        var response = await _service.GetAllAsync(ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<List<RecargoResponseDto>>();
    }

    /// <summary>
    /// Obtiene un recargo por su identificador.
    /// </summary>
    /// <param name="id">Identificador del recargo.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Recargo encontrado.</returns>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<RecargoResponseDto>> GetById([SwaggerParameter("Identificador del recargo.")] long id, CancellationToken ct)
    {
        var response = await _service.GetByIdAsync(id, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<RecargoResponseDto>();
    }

    /// <summary>
    /// Crea un nuevo recargo.
    /// </summary>
    /// <param name="request">Datos del recargo que se desea crear.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Recargo creado.</returns>
    [HttpPost]
    public async Task<ActionResult<RecargoResponseDto>> Create([FromBody] RecargoCreateRequestDto request, CancellationToken ct)
    {
        var response = await _service.CreateAsync(request, ct);

        return response.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, response.Value)
            : response.Error.ToHttpResult<RecargoResponseDto>();
    }

    /// <summary>
    /// Actualiza los datos de un recargo.
    /// </summary>
    /// <param name="id">Identificador del recargo.</param>
    /// <param name="request">Nuevos datos del recargo.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Recargo actualizado.</returns>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<RecargoResponseDto>> Update([SwaggerParameter("Identificador del recargo.")] long id, [FromBody] RecargoUpdateRequestDto request, CancellationToken ct)
    {
        var response = await _service.UpdateAsync(id, request, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<RecargoResponseDto>();
    }

    /// <summary>
    /// Desactiva un recargo mediante una baja lógica.
    /// </summary>
    /// <param name="id">Identificador del recargo.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Confirmación de la operación.</returns>
    [HttpDelete("{id:long}")]
    public async Task<ActionResult<bool>> Delete([SwaggerParameter("Identificador del recargo.")] long id, CancellationToken ct)
    {
        var response = await _service.DeleteAsync(id, ct);

        return response.IsSuccess
            ? NoContent()
            : response.Error.ToHttpResult<bool>();
    }
}