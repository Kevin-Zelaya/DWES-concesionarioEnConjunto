using APIAlquilerVehiculos.DTOs.Marcas;
using APIAlquilerVehiculos.Services.Marcas;
using Microsoft.AspNetCore.Mvc;

namespace APIAlquilerVehiculos.Controllers;

/// <summary>
/// Controlador encargado de gestionar las operaciones relacionadas con las marcas.
/// </summary>
[Route("api/marcassss")]
[ApiController]
public class MarcaController(IMarcaService service) : ControllerBase
{
    /// <summary>
    /// Servicio encargado de gestionar la lógica de negocio de las marcas.
    /// </summary>
    private readonly IMarcaService _service = service;

    /// <summary>
    /// Obtiene todas las marcas activas.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var resultado = await _service.GetAllAsync();

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error.Message);

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtiene las marcas de forma paginada.
    /// </summary>
    [HttpGet("paginated")]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var resultado = await _service.GetPaginatedMarcasAsync(pageNumber, pageSize);

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error.Message);

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtiene una marca por su identificador.
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var resultado = await _service.GetByIdAsync(id);

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error.Message);

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtiene una marca por su nombre.
    /// </summary>
    [HttpGet("nombre/{nombre}")]
    public async Task<IActionResult> GetByNombre(string nombre)
    {
        var resultado = await _service.GetByNombreAsync(nombre);

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error.Message);

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Crea una nueva marca.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMarcaDto dto)
    {
        var resultado = await _service.CreateAsync(dto);

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error.Message);

        return CreatedAtAction(
            nameof(GetById),
            new { id = resultado.Value.Id },
            resultado.Value);
    }

    /// <summary>
    /// Actualiza los datos de una marca existente.
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateMarcaDto dto)
    {
        var resultado = await _service.UpdateAsync(id, dto);

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error.Message);

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Elimina lógicamente una marca.
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var resultado = await _service.DeleteAsync(id);

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error.Message);

        return NoContent();
    }
}