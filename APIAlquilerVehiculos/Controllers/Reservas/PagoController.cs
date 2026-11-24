
using APIAlquilerVehiculos.DTOs.Pagos;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Services.Pagos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace APIAlquilerVehiculos.Controllers.Pagos;

[ApiController]
[Route("api/pagos")]
public class PagoController(
    IPagoService _service
) : ControllerBase
{
    // =========================================================
    // CREATE
    // =========================================================

    [HttpPost("reserva/{id:long}")]
    [Authorize(Roles = Roles.Cliente)]
    [SwaggerOperation(
        Summary = "Realizar un pago de una reserva",
        Description = "Procesa un intento de pago mediante tarjeta de crédito."
    )]
    [SwaggerResponse(201, "Intento de pago registrado correctamente.", typeof(PagoResponse))]
    [SwaggerResponse(400, "Los datos proporcionados no son válidos.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos para realizar este pago.")]
    [SwaggerResponse(404, "No se ha encontrado la reserva.")]
    [SwaggerResponse(409, "La reserva no se encuentra pendiente de pago.")]
    public async Task<ActionResult<PagoResponse>> CreatePago(
        [SwaggerParameter("Identificador de la reserva.")]
        long id,
        [FromBody] CrearPagoRequestDto request,
        CancellationToken ct)
    {
        var response = await _service.CrearPagoAsync(id, request, ct);

        return response.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, response.Value)
            : response.Error.ToHttpResult<PagoResponse>();
    }
}
