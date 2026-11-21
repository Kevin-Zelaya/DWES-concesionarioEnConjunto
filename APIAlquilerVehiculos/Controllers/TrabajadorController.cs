using APIAlquilerVehiculos.Dtos.Trabajadores;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Services.Trabajadores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIAlquilerVehiculos.Controllers;

/// <summary>
/// Controlador encargado de gestionar las operaciones relacionadas con los trabajadores.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class TrabajadorController(ITrabajadorService service) : ControllerBase
{
    /// <summary>
    /// Servicio encargado de gestionar la lógica de negocio de los trabajadores.
    /// </summary>
    private readonly ITrabajadorService _service = service;

    /// <summary>
    /// Obtiene los trabajadores de forma paginada.
    /// </summary>
    /// <param name="page">Número de página (empieza en 1).</param>
    /// <param name="pageSize">Cantidad de elementos por página (máximo 100).</param>
    /// <param name="incluirInactivos">Si es true, incluye los trabajadores dados de baja.</param>
    /// <param name="tipo">Filtra por tipo de trabajador (Operaciones o Mecanico).</param>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Administrador},{Roles.Responsable}")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool incluirInactivos = false,
        [FromQuery] TipoTrabajador? tipo = null)
    {
        var resultado = await _service.GetAllPagedAsync(page, pageSize, incluirInactivos, tipo);

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
    public async Task<IActionResult> GetById(long id)
    {
        var resultado = await _service.GetByIdAsync(id);

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error);

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Crea un nuevo trabajador.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{Roles.Administrador}")]
    public async Task<IActionResult> Create([FromBody] CreateTrabajadorDto dto)
    {
        var resultado = await _service.AddAsync(dto);

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error);

        return CreatedAtAction(
            nameof(GetById),
            new { id = resultado.Value.Id },
            resultado.Value);
    }

    /// <summary>
    /// Actualiza los datos de un trabajador existente.
    /// </summary>
    [HttpPut("{id:long}")]
    [Authorize(Roles = $"{Roles.Administrador}")]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateTrabajadorDto dto)
    {
        var resultado = await _service.UpdateAsync(id, dto);

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error);

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Da de baja lógicamente a un trabajador.
    /// </summary>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = $"{Roles.Administrador}")]
    public async Task<IActionResult> Delete(long id)
    {
        var resultado = await _service.DeleteAsync(id);

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error);

        return NoContent();
    }
}
