using APIAlquilerVehiculos.Dtos.Auth;
using APIAlquilerVehiculos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace APIAlquilerVehiculos.Controlleres;

/// <summary>
/// Controller de autenticacion usando ASP.NET Core Identity.
/// Gestiona el registro de usuarios, inicio de sesión (generación de tokens JWT) y obtención del perfil actual.
/// </summary>
[ApiController]
[Route("api/auth")]
[SwaggerTag("Gestion de usuarios y Tokens")]
public class AuthController (IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    // UserManager<User> ==> gestiona CRUD de usuarios (crear, buscar, actualizar, eliminar, gestionar roles)
    // SignInManager<User> ==> gestiona las operaciones de login (verificar contraseña, bloqueo, login externo)
    
    /// <summary>
    /// Registra un usuario Cliente nuevo.
    /// Uso de Identity
    /// </summary>
    [HttpPost("signup")]
    [SwaggerOperation(
        Summary = "Registra un usuario cliente nuevo",
        Description = "Devuelve los datos del usuario y cliente registrados",
        OperationId = "Signup",
        Tags = new[] { "Usuarios" }
    )]
    //[SwaggerRequestExample(typeof(SignupDto), typeof(SignupRequestExample))]
    [SwaggerResponse(201, "Datos del usuario", typeof(UsuarioResponseDto))]
    [SwaggerResponse(400, "Parametros invalidos", typeof(ProblemDetails))]
    public async Task<IActionResult> Signup([FromBody, SwaggerRequestBody("Datos de registro del cliente", Required = true)] SignupDto request)
    {
        var result = await authService.SignupAsync(request);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCurrentUser), new { }, result.Value)
            : StatusCode((int)result.Error.StatusCode, result.Error);
    }

    /// <summary>
    /// Inicia sesion verificando las credenciales y genera un Token JWT para autenticar futuras peticiones.
    /// </summary>
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Inicia sesión de un usuario ya registrado",
        Description = "Devuelve el Token con informacion del usuario y el ID del usuario",
        OperationId = "Login",
        Tags = new[] { "Usuarios" }
    )]
    //[SwaggerRequestExample(typeof(LoginDto), typeof(LoginRequestExample))]
    [SwaggerResponse(200, "Token y su fecha de expiración y datos del usuario", typeof(AuthResponseDto))]
    [SwaggerResponse(401, "Credenciales invalidas", typeof(ProblemDetails))]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody, SwaggerRequestBody("Email y contraseña de usuario", Required = true)] LoginDto request)
    
    {
        var result = await authService.LoginAsync(request);

        return result.IsSuccess
            ? Ok(result.Value)
            : Unauthorized(result.Error);
    }

    /// <summary>
    /// Devuelve el usuario autenticado actual.
    /// </summary>
    [Authorize] // Exige que la petición incluya un token JWT válido en las cabeceras
    [HttpGet("me")]
    [SwaggerOperation(
        Summary = "Perfil de datos del usuario",
        Description = "Devuelve el usuario autenticado actual",
        OperationId = "GetCurrent",
        Tags = new[] { "User" }
    )]
    [SwaggerResponse(200, "Datos del usuario y fecha de creación y de inicio de sesión", typeof(UsuarioResponseDto))]
    [SwaggerResponse(404, "Usuario no encontrado")]
    [SwaggerResponse(401, "Usuario no autorizado")]
    public async Task<ActionResult<UsuarioResponseDto>> GetCurrentUser()
    {
        var result = await authService.GetCurrentUserAsync(User);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(result.Error);
    } 
}
