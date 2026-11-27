using System.Security.Claims;
using APIAlquilerVehiculos.Dtos.Email;
using APIAlquilerVehiculos.Dtos.Taller;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Services.Email;
using APIAlquilerVehiculos.Services.Taller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Controllers;

/// <summary>
/// Controlador encargado de gestionar los partes de taller y el mantenimiento de los vehículos.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TallerController(IParteService service, AppDbContext context) : ControllerBase
{
    /// <summary>
    /// Obtiene todos los partes de taller registrados.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Administrador},{Roles.Responsable},{Roles.Mecanico},{Roles.Operario}")]
    public async Task<IActionResult> GetAll()
    {
        var resultado = await service.GetAllAsync();

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error);

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Obtiene el detalle de un parte de taller por su identificador.
    /// </summary>
    [HttpGet("{id:long}")]
    [Authorize(Roles = $"{Roles.Administrador},{Roles.Responsable},{Roles.Mecanico},{Roles.Operario}")]
    public async Task<IActionResult> GetById(long id)
    {
        var resultado = await service.GetByIdAsync(id);

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error);

        return Ok(resultado.Value);
    }

    /// <summary>
    /// Abre un nuevo parte de taller para un vehículo y lo marca en mantenimiento (solo mecánicos).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{Roles.Mecanico}")]
    public async Task<IActionResult> AbrirParte([FromBody] AbrirParteRequestDto request)
    {
        var trabajadorId = await ObtenerTrabajadorIdAutenticadoAsync();
        if (trabajadorId is null)
            return Forbid();

        var resultado = await service.AbrirParteAsync(trabajadorId.Value, request);

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error);

        return CreatedAtAction(
            nameof(GetById),
            new { id = resultado.Value.Id },
            resultado.Value);
    }

    /// <summary>
    /// Cierra un parte de taller abierto y devuelve el vehículo a estado disponible (solo mecánicos).
    /// </summary>
    [HttpPatch("{id:long}/cerrar")]
    [Authorize(Roles = $"{Roles.Mecanico}")]
    public async Task<IActionResult> CerrarParte(long id)
    {
        var trabajadorId = await ObtenerTrabajadorIdAutenticadoAsync();
        if (trabajadorId is null)
            return Forbid();

        var resultado = await service.CerrarParteAsync(trabajadorId.Value, id);

        if (resultado.IsFailure)
            return StatusCode((int)resultado.Error.StatusCode, resultado.Error);

        return Ok(resultado.Value);
    }

    // TEMPORAL: Endpoint de prueba de envío de correo (borrar antes del commit final)
    [HttpPost("test-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ProbarEmail(
        [FromServices] INotificationEmailService notificationEmail)
    {
        var destino = "funkoapi52@gmail.com";

        await notificationEmail.EnviarAsync(new ParteTallerEmailDto(
            EmailDestino: destino,
            NombreCliente: "Aitor",
            CodigoReferencia: "RES-2026-001",
            VehiculoOriginal: "Seat León (1234BCD)",
            DescripcionAveria: "Revisión de frenos en taller",
            ResolucionAplicada: "Cambio de vehículo equivalente",
            DetalleResolucion: "Te hemos asignado un Volkswagen Golf (5678FGH) sin coste adicional."
        ));

        return Ok(new { mensaje = $"Correo encolado hacia {destino}" });
    }

    /// <summary>
    /// Resuelve el ID del trabajador autenticado a partir de los claims del JWT.
    /// </summary>
    private async Task<long?> ObtenerTrabajadorIdAutenticadoAsync()
    {
        var trabajadorClaim = User.FindFirstValue("trabajadorId");
        if (long.TryParse(trabajadorClaim, out var trabajadorIdDirecto))
            return trabajadorIdDirecto;

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub");
        if (!long.TryParse(userIdClaim, out var usuarioId))
            return null;

        var trabajador = await context.Trabajadores
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IdUsuario == usuarioId && t.IsActivo);

        return trabajador?.Id;
    }
}