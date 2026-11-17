using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using APIAlquilerVehiculos.Dtos.Auth;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace APIAlquilerVehiculos.Services.Auth;

public class AuthService(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, AppDbContext dbContext, IConfiguration configuration, ILogger<AuthService> logger) : IAuthService
{
   public async Task<Result<UsuarioResponseDto, ProblemDetails>> SignupAsync(SignupDto request)
    {
        // 1. Validaciones de negocio 
        var validacionDoc = ValidarDocumentacion(request);
        if (validacionDoc.IsFailure)
            return Result.Failure<UsuarioResponseDto, ProblemDetails>(validacionDoc.Error);

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            var user = new Usuario
            {
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.Telefono,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, request.Contraseña);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                logger.LogWarning("Error de registro de Cliente");
                return Result.Failure<UsuarioResponseDto, ProblemDetails>(new ProblemDetails
                {
                    Title = "Error de registro",
                    Detail = string.Join(", ", result.Errors.Select(e => e.Description)),
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var roleResult = await userManager.AddToRoleAsync(user, Roles.Cliente);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return Result.Failure<UsuarioResponseDto, ProblemDetails>(new ProblemDetails
                {
                    Title = "Error asignando rol",
                    Status = StatusCodes.Status500InternalServerError
                });
            }

            var cliente = new Cliente
            {
                IdUsuario = user.Id,
                Nombre = request.Nombre,
                Apellidos = request.Apellidos,
                FechaNacimiento = DateTime.SpecifyKind(request.FechaNacimiento, DateTimeKind.Utc),
                Dni = request.Dni?.ToUpperInvariant(),
                Nie = request.Nie?.ToUpperInvariant(),
                Pasaporte = request.Pasaporte?.ToUpperInvariant(),
                NumeroPermisoConducir = request.NumeroPermisoConducir,
                CreatedAt = DateTime.UtcNow,
                IsActivo = true
            };

            dbContext.Cliente.Add(cliente);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            logger.LogInformation("Cliente {ClienteId} y Usuario {UserId} registrados correctamente", cliente.Id, user.Id);

            return Result.Success<UsuarioResponseDto, ProblemDetails>(new UsuarioResponseDto(
                Id: user.Id,
                Email: user.Email ?? string.Empty,
                Telefono: user.PhoneNumber ?? string.Empty,
                Rol: Roles.Cliente,
                ClienteId: cliente.Id,
                TrabajadorId: null,
                CreatedAt: user.CreatedAt
            ));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error inesperado durante el registro del cliente");
            return Result.Failure<UsuarioResponseDto, ProblemDetails>(new ProblemDetails
            {
                Title = "Error interno durante el registro",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    public async Task<Result<AuthResponseDto, ProblemDetails>> LoginAsync(LoginDto request)
    {
        // Cargamos Usuario junto con Cliente y Trabajador para evitar NullReferenceException y comprobar IsActivo
        var user = await userManager.Users
            .Include(u => u.Cliente)
            .Include(u => u.Trabajador)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
        {
            logger.LogWarning("Credenciales invalidas del usuario");
            return Result.Failure<AuthResponseDto, ProblemDetails>(CredencialesInvalidas());
        }

        // Comprobamos que su ficha asociada siga activa (baja lógica)
        if ((user.Cliente is not null && !user.Cliente.IsActivo) ||
            (user.Trabajador is not null && !user.Trabajador.IsActivo))
        {
            logger.LogWarning("Intento de login de cuenta desactivada {UserId}", user.Id);
            return Result.Failure<AuthResponseDto, ProblemDetails>(CredencialesInvalidas());
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Contraseña, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            logger.LogWarning("Contraseña incorrecta del usuario {UserId}", user.Id);
            return Result.Failure<AuthResponseDto, ProblemDetails>(CredencialesInvalidas());
        }

        var roles = await userManager.GetRolesAsync(user);
        var rolPrincipal = roles.FirstOrDefault() ?? Roles.Cliente;

        var (tokenString, expiresAt) = GenerarJwt(user, roles, rolPrincipal);

        logger.LogInformation("Usuario {UserId} ha iniciado sesion", user.Id);

        var usuarioDto = new UsuarioResponseDto(
            Id: user.Id,
            Email: user.Email ?? string.Empty,
            Telefono: user.PhoneNumber ?? string.Empty,
            Rol: rolPrincipal,
            ClienteId: user.Cliente?.Id,
            TrabajadorId: user.Trabajador?.Id,
            CreatedAt: user.CreatedAt
        );

        return Result.Success<AuthResponseDto, ProblemDetails>(
            new AuthResponseDto(tokenString, expiresAt, usuarioDto)
        );
    }

    public async Task<Result<UsuarioResponseDto, ProblemDetails>> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier) 
                        ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!long.TryParse(userIdStr, out var userId))
        {
            return Result.Failure<UsuarioResponseDto, ProblemDetails>(new ProblemDetails
            {
                Title = "Usuario no encontrado",
                Status = StatusCodes.Status404NotFound
            });
        }

        var user = await userManager.Users
            .Include(u => u.Cliente)
            .Include(u => u.Trabajador)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            logger.LogWarning("Usuario {UserId} no encontrado", userId);
            return Result.Failure<UsuarioResponseDto, ProblemDetails>(new ProblemDetails
            {
                Title = "Usuario no encontrado",
                Status = StatusCodes.Status404NotFound
            });
        }

        var roles = await userManager.GetRolesAsync(user);
        var rolPrincipal = roles.FirstOrDefault() ?? Roles.Cliente;

        return Result.Success<UsuarioResponseDto, ProblemDetails>(new UsuarioResponseDto(
            Id: user.Id,
            Email: user.Email ?? string.Empty,
            Telefono: user.PhoneNumber ?? string.Empty,
            Rol: rolPrincipal,
            ClienteId: user.Cliente?.Id,
            TrabajadorId: user.Trabajador?.Id,
            CreatedAt: user.CreatedAt
        ));
    }

    // --- Métodos privados auxiliares ---

    private static UnitResult<ProblemDetails> ValidarDocumentacion(SignupDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Dni) &&
            string.IsNullOrWhiteSpace(request.Nie) &&
            string.IsNullOrWhiteSpace(request.Pasaporte))
        {
            return UnitResult.Failure(new ProblemDetails
            {
                Title = "Documentación incompleta",
                Detail = "Debe proporcionar al menos un documento de identidad (DNI, NIE o Pasaporte).",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (string.IsNullOrWhiteSpace(request.Dni) &&
            string.IsNullOrWhiteSpace(request.Nie) &&
            !string.IsNullOrWhiteSpace(request.Pasaporte) &&
            string.IsNullOrWhiteSpace(request.NumeroPermisoConducir))
        {
            return UnitResult.Failure(new ProblemDetails
            {
                Title = "Permiso de conducir requerido",
                Detail = "Si se registra únicamente con pasaporte, debe indicar el número de permiso de conducir.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // Aquí llamas a tus validadores reutilizables de formato/letra de DNI, NIE y Pasaporte

        return UnitResult.Success<ProblemDetails>();
    }

    private (string Token, DateTime ExpiresAt) GenerarJwt(Usuario user, IList<string> roles, string rolPrincipal)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(ClaimTypes.Role, rolPrincipal)
        };

        if (roles.Contains(Roles.Mecanico))
            claims.Add(new Claim("tipo_trabajo", "Mecanico"));

        if (roles.Contains(Roles.Operario))
            claims.Add(new Claim("tipo_trabajo", "Operario"));

        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret no configurado"));

        var expiresAt = DateTime.UtcNow.AddMinutes(15);
        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"],
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        });

        return (tokenHandler.WriteToken(token), expiresAt);
    }

    private static ProblemDetails CredencialesInvalidas() => new()
    {
        Title = "Credenciales invalidas",
        Status = StatusCodes.Status401Unauthorized
    }; 
}