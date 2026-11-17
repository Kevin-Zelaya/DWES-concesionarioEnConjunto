using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using APIAlquilerVehiculos.Dtos.Auth;
using APIAlquilerVehiculos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace APIAlquilerVehiculos.Controller;



/// <summary>
/// Controller de autenticacion usando ASP.NET Core Identity.
/// Gestiona el registro de usuarios, inicio de sesión (generación de tokens JWT) y obtención del perfil actual.
/// </summary>
[ApiController]
[Route("api/auth")]
[SwaggerTag("Gestion de usuarios y Tokens")]
public class AuthController (UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, IConfiguration configuration, ILogger<AuthController> logger) : ControllerBase
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
        Description = "Devuelve el ID, correo electronico y nombre de usuario del nuevo usuario",
        OperationId = "Signup",
        Tags = new[] { "Usuarios" }
    )]
    //[SwaggerRequestExample(typeof(SignupDto), typeof(SignupRequestExample))]
    [SwaggerResponse(201, "Datos del usuario", typeof(IActionResult))]
    [SwaggerResponse(400, "Parametros invalidos", typeof(ProblemDetails))]
    public async Task<IActionResult> Signup([FromBody, SwaggerRequestBody("Email, Nombre y Apellidos, contrasena, telefono, Fecha de nacimiento, Documentación y número de permiso de conducir del usuario")] 
        SignupDto request)
    {
        var user = new Usuario
        { //Datos por defecto de entity
            Email = request.Email,
            PhoneNumber = request.Telefono,
            
            Nombre = request.Nombre,

        };
        
        //Guardamos y creamos el nuevo usuario con las reglas de contraseña
        var result = await userManager.CreateAsync(user, request.Contraseña);
        logger.LogInformation("Creando usuario");

        
        if (!result.Succeeded)
        {
            logger.LogWarning("Error de registro de usuario");
            return BadRequest(new ProblemDetails
            {
                Title = "Error de registro",
                Detail = string.Join(", ", result.Errors.Select(e => e.Description)),
                Status = StatusCodes.Status400BadRequest
            });
        }
        // A todo nuevo usuario se le asigna el rol Cliente
        await userManager.AddToRoleAsync(user, Roles.Cliente);
        logger.LogInformation("Asignando rol cliente");
        // Retornamos 201 con sus datos
        return CreatedAtAction(nameof(GetCurrentUser), new { }, new
        {
            user.Id,
            user.UserName,
            user.Email
        });
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
    [SwaggerResponse(200, "Datos del usuario", typeof(IActionResult))]
    [SwaggerResponse(401, "Credenciales invalidas", typeof(ProblemDetails))]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody, SwaggerRequestBody("usuario y contraseña", Required = true)] LoginDto request)
    
    {
        //Buscamos al usuario por su nombre de usuario
        var user = await  userManager.FindByEmailAsync(request.Email);
        
        if (user is null)
        {
            logger.LogWarning("Credenciales invalidas del usuario");
            return Unauthorized(new ProblemDetails
            {
                Title = "Credenciales invalidas",
                Status = StatusCodes.Status401Unauthorized
            });
        }
        logger.LogInformation("Usuario {UserId} encontrado", user.Id);
        // Verificamos que la contraseña sea correcta (bloqueando la cuenta si excede los intentos permitidos = 5)
        var result = await signInManager.CheckPasswordSignInAsync(user, request.Contraseña, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            logger.LogWarning("Contraseña incorrecta del usuario {UserId}", user.Id);
            return Unauthorized(new ProblemDetails
            {
                Title = "Credenciales invalidas",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        await userManager.UpdateAsync(user);  
        logger.LogInformation("Usuario {UserId} a iniciado sesion", user.Id);
        
        // Generar JWT  
        // Obtenemos los roles asociados al usuario para incluirlos dentro de las credenciales del token
        var roles = await userManager.GetRolesAsync(user);
        var rolPrincipal = roles.FirstOrDefault() ?? Roles.Cliente;
        
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(ClaimTypes.Role, rolPrincipal),
        };
        if (roles.Contains(Roles.Mecanico)) 
        {
            claims.Add(new Claim("tipo_trabajo", "Mecanico")); 
        }
        if (roles.Contains(Roles.Operario)) 
        {
            claims.Add(new Claim("tipo_trabajo", "Operario")); 
        }
        
        
        // Obtenemos y validamos la clave secreta (configurada en appsettings.Development.json)
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret no configurado"));

        var expiresAt = DateTime.UtcNow.AddMinutes(15); // El token expira en 15 minutos
        
        //Configuramos los Claims ==> información del usuario que viaja dentro del token. Ademas de su tiempo de expiración
        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"],
            Expires = expiresAt, 
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        });
        
        if (roles.Contains(Roles.Cliente))
        {
            var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == user.Id); // Ajusta según tu propiedad
            var clienteId = cliente?.Id;
        }
        else if (roles.Contains(Roles.Operario) || roles.Contains(Roles.Mecanico) || roles.Contains(Roles.Responsable))
        {
            var trabajador = await context.Trabajadores.FirstOrDefaultAsync(t => t.UsuarioId == user.Id); // Ajusta según tu propiedad
            var trabajadorId = trabajador?.Id;
        }
        
        // Construimos el DTO del usuario (ajusta los parámetros según los campos que tenga tu UsuarioResponseDto)
        var usuarioDto = new UsuarioResponseDto(
            Id: user.Id,
            Email: user.Email ?? string.Empty,
            Telefono: user.PhoneNumber ?? string.Empty,
            Rol: rolPrincipal,
            ClienteId: user.C,
            TrabajadorId: user.Trabajador.Id ?? null,
            CreatedAt: user.CreatedAt
            // Si tu UsuarioResponseDto tiene más campos como Nombre, Activo, ClienteId o TrabajadorId, pásalos aquí
        );

        // Retornamos  AuthResponseDto 
        var respuesta = new AuthResponseDto(
            Token: tokenHandler.WriteToken(token),
            ExpiresAt: expiresAt,
            Usuario: usuarioDto
        );

        return Ok(respuesta);
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
    public async Task<ActionResult<UsuarioResponseDto>> GetCurrentUser()
    {
        // Extraemos el usuario actual a partir del contexto de seguridad de la petición
        var user = await userManager.GetUserAsync(User);
        if (user is null) {logger.LogWarning("Usuario NO encontrado"); return NotFound();}

        // Consultamos sus roles actuales
        var roles = await userManager.GetRolesAsync(user);
        
        if(user.UserName is null ||  user.Email is null){logger.LogWarning("Usuario NO encontrado"); return NotFound();}
        
        // Retornamos un objeto plano con la información de perfil
        return Ok(new UsuarioResponseDto(
            user.Id,
            user.Email ?? string.Empty,
            user.PhoneNumber ?? string.Empty,
            roles,
            user.ClienteId,
            user.TrabajadorId,
            user.CreatedAt
        ));
    } 
}
