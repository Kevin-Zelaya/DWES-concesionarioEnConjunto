using APIAlquilerVehiculos.Dtos.Vehiculo;
using APIAlquilerVehiculos.Services.Vehiculo;
using Microsoft.AspNetCore.Mvc;

namespace APIAlquilerVehiculos.Controllers;

[ApiController]
[Route("api/[Vehiculos]")]
public class VehiculosController(IVehiculoService service) : ControllerBase
{
    /// <summary>
    /// Obtiene todos los vehículos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<VehiculoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await service.GetAllAsync(ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode((int)result.Error.StatusCode, result.Error);
    }

    /// <summary>
    /// Obtiene un vehículo por id
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VehiculoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode((int)result.Error.StatusCode, result.Error);
    }

    /// <summary>
    /// Obtiene un vehículo por matrícula
    /// </summary>
    [HttpGet("matricula/{matricula}")]
    [ProducesResponseType(typeof(VehiculoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByMatricula(string matricula, CancellationToken ct)
    {
        var result = await service.GetByMatriculaAsync(matricula, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode((int)result.Error.StatusCode, result.Error);
    }

    /// <summary>
    /// Crea un vehículo
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(VehiculoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] VehiculoCreateDto dto,
        CancellationToken ct)
    {
        var result = await service.CreateAsync(dto, ct);

        if (result.IsFailure)
            return StatusCode((int)result.Error.StatusCode, result.Error);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value.Id },
            result.Value);
    }

    /// <summary>
    /// Actualiza un vehículo
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(VehiculoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] VehiculoUpdateDto dto,
        CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, dto, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode((int)result.Error.StatusCode, result.Error);
    }

    /// <summary>
    /// Elimina un vehículo
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var result = await service.DeleteAsync(id, ct);

        return result.IsSuccess
            ? NoContent()
            : StatusCode((int)result.Error.StatusCode, result.Error);
    }

    /// <summary>
    /// Obtiene una página de vehículos
    /// </summary>
    [HttpGet("paginated")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetPaginatedVehiculosAsync(
            pageNumber,
            pageSize,
            ct);

        if (result.IsFailure)
            return StatusCode((int)result.Error.StatusCode, result.Error);

        return Ok(new
        {
            Items = result.Value.items,
            Count = result.Value.count,
            TotalPages = result.Value.totalPages,
            HasPreviousPage = result.Value.HasPreviousPage,
            HasNextPage = result.Value.HasNextPage
        });
    }
}