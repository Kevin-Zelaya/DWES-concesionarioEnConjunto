using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// Controlador encargado de gestionar los extras.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExtraController(IExtraService _service) : ControllerBase
{
    /// <summary>
    /// Obtiene todos los extras activos.
    /// </summary>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Listado de extras.</returns>
    [HttpGet]
    public async Task<ActionResult<List<ExtraResponseDto>>> GetAll(CancellationToken ct)
    {
        var response = await _service.GetAllAsync(ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<List<ExtraResponseDto>>();
    }

    /// <summary>
    /// Obtiene un extra por su identificador.
    /// </summary>
    /// <param name="id">Identificador del extra.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Extra encontrado.</returns>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ExtraResponseDto>> GetById(
        [SwaggerParameter("Identificador del extra.")]
        long id,
        CancellationToken ct)
    {
        var response = await _service.GetByIdAsync(id, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<ExtraResponseDto>();
    }

    /// <summary>
    /// Crea un nuevo extra.
    /// </summary>
    /// <param name="request">Datos del extra que se desea crear.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Extra creado.</returns>
    [HttpPost]
    public async Task<ActionResult<ExtraResponseDto>> Create(
        [FromBody] ExtraCreateRequestDto request,
        CancellationToken ct)
    {
        var response = await _service.CreateAsync(request, ct);

        return response.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, response.Value)
            : response.Error.ToHttpResult<ExtraResponseDto>();
    }

    /// <summary>
    /// Actualiza los datos de un extra.
    /// </summary>
    /// <param name="id">Identificador del extra.</param>
    /// <param name="request">Nuevos datos del extra.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Extra actualizado.</returns>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ExtraResponseDto>> Update(
        [SwaggerParameter("Identificador del extra.")]
        long id,
        [FromBody] ExtraUpdateRequestDto request,
        CancellationToken ct)
    {
        var response = await _service.UpdateAsync(id, request, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<ExtraResponseDto>();
    }

    /// <summary>
    /// Desactiva un extra mediante una baja lógica.
    /// </summary>
    /// <param name="id">Identificador del extra.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Confirmación de la operación.</returns>
    [HttpDelete("{id:long}")]
    public async Task<ActionResult<bool>> Delete(
        [SwaggerParameter("Identificador del extra.")]
        long id,
        CancellationToken ct)
    {
        var response = await _service.DeleteAsync(id, ct);

        return response.IsSuccess
            ? NoContent()
            : response.Error.ToHttpResult<bool>();
    }
}