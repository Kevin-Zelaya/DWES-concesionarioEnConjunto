using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Models.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/reservas")]
public class ReservaController(
    IReservaService _service
) : ControllerBase
{
    // =========================================================
    // CREATE
    // =========================================================

    [HttpPost]
    [Authorize(Roles = $"{Roles.Operario},{Roles.Responsable}")]
    [SwaggerOperation(
        Summary = "Crear una reserva",
        Description = "Crea una nueva reserva."
    )]
    [SwaggerResponse(201, "Reserva creada correctamente.", typeof(Reserva))]
    [SwaggerResponse(400, "Los datos proporcionados no son válidos.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos para crear reservas.")]
    [SwaggerResponse(409, "Conflicto al crear la reserva.")]
    public async Task<ActionResult<Reserva>> CreateReserva(
        [FromBody] ReservaCreateRequestDto reserva,
        CancellationToken ct)
    {
        // Busca el id que se guarda como subject del usuario, si no lo tiene retorna un unauthorize
        if (!long.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId))
            return Unauthorized();
        
        var response = await _service.CreateReservaAsync(reserva, userId, ct);

        return response.IsSuccess
            ? CreatedAtRoute(
                "GetReservaByCodigoReferencia",
                new { codigo = response.Value.CodigoRefencia },
                response.Value)
            : response.Error.ToHttpResult<Reserva>();
    }

    // =========================================================
    // READ
    // =========================================================

    [HttpGet("{id:long}")]
    [Authorize(Roles = $"{Roles.Cliente},{Roles.Operario},{Roles.Responsable}")]
    [SwaggerOperation(
        Summary = "Obtener una reserva por ID",
        Description = "Devuelve los datos de una reserva identificada por su ID."
    )]
    [SwaggerResponse(200, "Reserva encontrada.", typeof(Reserva))]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos para consultar esta reserva.")]
    [SwaggerResponse(404, "No se ha encontrado la reserva.")]
    public async Task<ActionResult<Reserva>> GetReservaById(
        [SwaggerParameter("Identificador de la reserva.")]
        long id,
        CancellationToken ct)
    {
        var response = await _service.GetReservaByIdAsync(id, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<Reserva>();
    }

    [HttpGet("codigo/{codigo}", Name = "GetReservaByCodigoReferencia")]
    [Authorize(Roles = $"{Roles.Cliente},{Roles.Operario},{Roles.Responsable}")]
    [SwaggerOperation(
        Summary = "Obtener una reserva por código de referencia",
        Description = "Devuelve los datos de una reserva mediante su código de referencia."
    )]
    [SwaggerResponse(200, "Reserva encontrada.", typeof(Reserva))]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos para consultar esta reserva.")]
    [SwaggerResponse(404, "No se ha encontrado la reserva.")]
    public async Task<ActionResult<Reserva>> GetReservaByCodigoReferencia(
        [SwaggerParameter("Código de referencia de la reserva.")]
        string codigo,
        CancellationToken ct)
    {
        var response = await _service.GetReservaByCodigoReferenciaAsync(codigo, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<Reserva>();
    }

    [HttpGet("all")]
    [Authorize(Roles = $"{Roles.Operario},{Roles.Mecanico},{Roles.Responsable}")]
    [SwaggerOperation(
        Summary = "Obtener todas las reservas",
        Description = "Devuelve el listado completo de reservas."
    )]
    [SwaggerResponse(200, "Listado de reservas obtenido correctamente.", typeof(List<Reserva>))]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos para consultar todas las reservas.")]
    public async Task<ActionResult<List<Reserva>>> GetAllReservas(
        CancellationToken ct)
    {
        var response = await _service.GetAllReservasAsync(ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<List<Reserva>>();
    }

    // =========================================================
    // PAGINATION
    // =========================================================

    [HttpGet]
    [Authorize(Roles = $"{Roles.Operario},{Roles.Mecanico},{Roles.Responsable}")]
    [SwaggerOperation(
        Summary = "Obtener reservas paginadas",
        Description = "Devuelve una página de reservas según el número de página y el tamaño solicitado."
    )]
    [SwaggerResponse(200, "Página obtenida correctamente.", typeof(PaginatedResouce<Reserva>))]
    [SwaggerResponse(400, "El número de página o el tamaño son inválidos.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos para consultar las reservas.")]
    public async Task<ActionResult<PaginatedResouce<Reserva>>> GetPaginatedReservas(
        [FromQuery, SwaggerParameter("Número de página (mínimo 1).")]
        int pageNumber = 1,

        [FromQuery, SwaggerParameter("Cantidad de elementos por página (mínimo 1).")]
        int pageSize = 5,

        CancellationToken ct = default)
    {
        Log.Information(
            "PageNumber: {PageNumber}, PageSize: {PageSize}",
            pageNumber, pageSize);

        if (pageNumber < 1 || pageSize < 1)
            return BadRequest("La página y el tamaño deben ser mayores que 0.");

        var response = await _service.GetPaginatedReservasAsync(pageNumber, pageSize, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<PaginatedResouce<Reserva>>();
    }

    [HttpGet("cliente/{id:long}")]
    [Authorize(Roles = $"{Roles.Cliente},{Roles.Operario},{Roles.Responsable}")]
    [SwaggerOperation(
        Summary = "Obtener reservas de un cliente",
        Description = "Devuelve las reservas de un cliente de forma paginada."
    )]
    [SwaggerResponse(200, "Reservas del cliente obtenidas correctamente.", typeof(PaginatedResouce<Reserva>))]
    [SwaggerResponse(400, "El número de página o el tamaño son inválidos.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos para consultar estas reservas.")]
    public async Task<ActionResult<PaginatedResouce<Reserva>>> GetPaginatedReservasByCustomerId(
        [SwaggerParameter("Identificador del cliente.")]
        long id,

        [FromQuery, SwaggerParameter("Número de página (mínimo 1).")]
        int pageNumber = 1,

        [FromQuery, SwaggerParameter("Cantidad de elementos por página (mínimo 1).")]
        int pageSize = 5,

        CancellationToken ct = default)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest("La página y el tamaño deben ser mayores que 0.");

        var response = await _service.GetPaginatedReservasByCustomerIdAsync(
            id, pageNumber, pageSize, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<PaginatedResouce<Reserva>>();
    }

    [HttpGet("trabajador/{id:long}")]
    [Authorize(Roles = $"{Roles.Operario},{Roles.Mecanico},{Roles.Responsable}")]
    [SwaggerOperation(
        Summary = "Obtener reservas de un trabajador",
        Description = "Devuelve las reservas asignadas a un trabajador de forma paginada."
    )]
    [SwaggerResponse(200, "Reservas del trabajador obtenidas correctamente.", typeof(PaginatedResouce<Reserva>))]
    [SwaggerResponse(400, "El número de página o el tamaño son inválidos.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos para consultar estas reservas.")]
    public async Task<ActionResult<PaginatedResouce<Reserva>>> GetPaginatedReservasByEmployeeId(
        [SwaggerParameter("Identificador del trabajador.")]
        long id,

        [FromQuery, SwaggerParameter("Número de página (mínimo 1).")]
        int pageNumber = 1,

        [FromQuery, SwaggerParameter("Cantidad de elementos por página (mínimo 1).")]
        int pageSize = 5,

        CancellationToken ct = default)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest("La página y el tamaño deben ser mayores que 0.");

        var response = await _service.GetPaginatedReservasByEmployeeIdAsync(
            id, pageNumber, pageSize, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<PaginatedResouce<Reserva>>();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    [HttpPatch("{codigo}/nota")]
    [Authorize(Roles = $"{Roles.Operario},{Roles.Responsable}")]
    [SwaggerOperation(
        Summary = "Actualizar las notas de una reserva",
        Description = "Modifica las notas de una reserva."
    )]
    [SwaggerResponse(200, "Notas actualizadas correctamente.", typeof(Reserva))]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos para modificar las notas.")]
    [SwaggerResponse(404, "Reserva no encontrada.")]
    public async Task<ActionResult<Reserva>> UpdateNota(
        [SwaggerParameter("Código de referencia de la reserva.")]
        string codigo,
        [FromBody] ReservaUpdateNotaRequestDto request,
        CancellationToken ct)
    {
        var response = await _service.UpdateNotaAsync(codigo, request, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<Reserva>();
    }

    [HttpPatch("{codigo}/fechas")]
    [Authorize(Roles = $"{Roles.Cliente},{Roles.Operario},{Roles.Responsable}")]
    [SwaggerOperation(
        Summary = "Actualizar las fechas de una reserva",
        Description = "Modifica las fechas de recogida y devolución de una reserva."
    )]
    [SwaggerResponse(200, "Fechas actualizadas correctamente.", typeof(Reserva))]
    [SwaggerResponse(400, "Las fechas proporcionadas no son válidas.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos para modificar estas fechas.")]
    [SwaggerResponse(404, "Reserva no encontrada.")]
    public async Task<ActionResult<Reserva>> UpdateFechas(
        [SwaggerParameter("Código de referencia de la reserva.")]
        string codigo,
        [FromBody] ReservaUpdateFechaRequestDto request,
        CancellationToken ct)
    {
        var response = await _service.UpdateFechasAsync(codigo, request, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<Reserva>();
    }

    [HttpPatch("{codigo}/estado")]
    [Authorize(Roles = $"{Roles.Operario},{Roles.Responsable}")]
    [SwaggerOperation(
        Summary = "Actualizar el estado de una reserva",
        Description = "Modifica el estado de una reserva."
    )]
    [SwaggerResponse(200, "Estado actualizado correctamente.", typeof(Reserva))]
    [SwaggerResponse(400, "El estado proporcionado no es válido.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos para modificar el estado.")]
    [SwaggerResponse(404, "Reserva no encontrada.")]
    public async Task<ActionResult<Reserva>> UpdateStatus(
        [SwaggerParameter("Código de referencia de la reserva.")]
        string codigo,
        [FromBody] ReservaUpdateStatusRequestDto request,
        CancellationToken ct)
    {
        var response = await _service.UpdateStatusAsync(codigo, request, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<Reserva>();
    }

    [HttpPatch("{codigo}/vehiculo")]
    [Authorize(Roles = $"{Roles.Operario},{Roles.Responsable}")]
    [SwaggerOperation(
        Summary = "Actualizar el vehículo de una reserva",
        Description = "Modifica el vehículo asignado a una reserva."
    )]
    [SwaggerResponse(200, "Vehículo actualizado correctamente.", typeof(Reserva))]
    [SwaggerResponse(400, "Los datos proporcionados no son válidos.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos para modificar el vehículo.")]
    [SwaggerResponse(404, "Reserva o vehículo no encontrados.")]
    [SwaggerResponse(409, "El vehículo no está disponible para las fechas de la reserva.")]
    public async Task<ActionResult<Reserva>> UpdateVehicle(
        [SwaggerParameter("Código de referencia de la reserva.")]
        string codigo,
        [FromBody] ReservaUpdateVehicleRequestDto request,
        CancellationToken ct)
    {
        var response = await _service.UpdateVehicleAsync(codigo, request, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<Reserva>();
    }

    // =========================================================
    // EXISTS
    // =========================================================

    [HttpGet("{id:long}/exists")]
    [Authorize(Roles = $"{Roles.Operario},{Roles.Mecanico},{Roles.Responsable}")]
    [SwaggerOperation(
        Summary = "Comprobar si existe una reserva",
        Description = "Comprueba si existe una reserva mediante su identificador."
    )]
    [SwaggerResponse(200, "Comprobación realizada correctamente.", typeof(bool))]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos para realizar esta comprobación.")]
    public async Task<ActionResult<bool>> Exists(
        [SwaggerParameter("Identificador de la reserva.")]
        long id,
        CancellationToken ct)
    {
        var response = await _service.ExistsAsync(id, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<bool>();
    }
}