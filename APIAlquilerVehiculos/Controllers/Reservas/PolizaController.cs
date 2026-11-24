using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// Controlador encargado de gestionar las pólizas.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PolizaController(IPolizaService _service) : ControllerBase
{
    /// <summary>
    /// Obtiene todas las pólizas activas.
    /// </summary>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Listado de pólizas.</returns>
    [HttpGet]
    public async Task<ActionResult<List<PolizaResponseDto>>> GetAll(CancellationToken ct)
    {
        var response = await _service.GetAllAsync(ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<List<PolizaResponseDto>>();
    }

    /// <summary>
    /// Obtiene una póliza por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la póliza.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Póliza encontrada.</returns>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<PolizaResponseDto>> GetById(
        [SwaggerParameter("Identificador de la póliza.")]
        long id,
        CancellationToken ct)
    {
        var response = await _service.GetByIdAsync(id, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<PolizaResponseDto>();
    }

    /// <summary>
    /// Crea una nueva póliza.
    /// </summary>
    /// <param name="request">Datos de la póliza que se desea crear.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Póliza creada.</returns>
    [HttpPost]
    public async Task<ActionResult<PolizaResponseDto>> Create(
        [FromBody] PolizaCreateRequestDto request,
        CancellationToken ct)
    {
        var response = await _service.CreateAsync(request, ct);

        return response.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, response.Value)
            : response.Error.ToHttpResult<PolizaResponseDto>();
    }

    /// <summary>
    /// Actualiza los datos de una póliza.
    /// </summary>
    /// <param name="id">Identificador de la póliza.</param>
    /// <param name="request">Nuevos datos de la póliza.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Póliza actualizada.</returns>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<PolizaResponseDto>> Update(
        [SwaggerParameter("Identificador de la póliza.")]
        long id,
        [FromBody] PolizaUpdateRequestDto request,
        CancellationToken ct)
    {
        var response = await _service.UpdateAsync(id, request, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<PolizaResponseDto>();
    }

    /// <summary>
    /// Desactiva una póliza mediante una baja lógica.
    /// </summary>
    /// <param name="id">Identificador de la póliza.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Confirmación de la operación.</returns>
    [HttpDelete("{id:long}")]
    public async Task<ActionResult<bool>> Delete(
        [SwaggerParameter("Identificador de la póliza.")]
        long id,
        CancellationToken ct)
    {
        var response = await _service.DeleteAsync(id, ct);

        return response.IsSuccess
            ? NoContent()
            : response.Error.ToHttpResult<bool>();
    }
}