using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using APIAlquilerVehiculos.Dtos.Auth;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Errors.Email;
using APIAlquilerVehiculos.Models;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace APIAlquilerVehiculos.Services.Auth;

public class AuthService(IValidator<SignupDto> validatorSignupAsync, UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, AppDbContext dbContext, IConfiguration configuration, ILogger<AuthService> logger) : IAuthService
{
    /// <summary>
    /// Registra un nuevo cliente guardando el registro y creando el nuevo usuario Cliente 
    /// </summary>
    /// <param name="request">DTO de petición datos y contraseña del cliente.</param>
    /// <returns>Devuelve los datos del cliente sin la contraseña</returns>
   public async Task<Result<UsuarioResponseDto, DomainError>> SignupAsync(SignupDto request)
    {
        // Ejecutamos FluentValidation
        var validationResult = await validatorSignupAsync.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errores = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            logger.LogWarning("Validación fallida en registro: {Errores}", errores);
            
            return Result.Failure<UsuarioResponseDto, DomainError>(AuthErrors.Validation(validationResult.ToDictionary()));
        }

        // Comprobaciones de unicidad en BD (Email, DNI, NIE o Pasaporte ya existentes)
        var dniNorm = request.Dni?.Trim().ToUpperInvariant();
        var nieNorm = request.Nie?.Trim().ToUpperInvariant();
        var pasaporteNorm = request.Pasaporte?.Trim().ToUpperInvariant();
        
        if (await dbContext.Cliente.AnyAsync(c =>
                (dniNorm != null && c.Dni == dniNorm) ||
                (nieNorm != null && c.Nie == nieNorm) ||
                (pasaporteNorm != null && c.Pasaporte == pasaporteNorm)))
        {
            var docDuplicado = dniNorm ?? nieNorm ?? pasaporteNorm ?? string.Empty;
            logger.LogInformation("Documentos duplicados: {Documento}", docDuplicado);
            return Result.Failure<UsuarioResponseDto, DomainError>(AuthErrors.DuplicateDocument(docDuplicado));
        }
        
        // Comprobamos que no existe otro Email identico
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            logger.LogInformation("Email duplicado: {Email}", request.Email);
            return Result.Failure<UsuarioResponseDto, DomainError>(
                AuthErrors.DuplicateEmail(request.Email)
            );
        }
        
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
                var erroresIdentity = result.Errors.Select(e => e.Description).ToList();
                logger.LogWarning("Error de registro de Cliente");
                return Result.Failure<UsuarioResponseDto, DomainError>(
                    AuthErrors.IdentityError(erroresIdentity)
                );
            }

            var roleResult = await userManager.AddToRoleAsync(user, Roles.Cliente);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                logger.LogError("No se pudo asignar el rol {Rol} al usuario {UserId}", Roles.Cliente, user.Id);

                return Result.Failure<UsuarioResponseDto, DomainError>(
                    AuthErrors.ErrorServer("Error interno al asignar los permisos del usuario.")
                );
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

            return Result.Success<UsuarioResponseDto, DomainError>(new UsuarioResponseDto(
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
            return Result.Failure<UsuarioResponseDto, DomainError>(AuthErrors.ErrorServer("Error inesperado durante el registro del cliente"));
        }
    }

    /// <summary>
    /// Autentica a un usuario con su correo electrónico y contraseña, verificando (ya sea usuario o trabajador) no haya sido dada de baja lógica.
    /// </summary>
    /// <param name="request">Email y contrasseña de usuario</param>
    /// <returns>
    /// Devuelve un token y su fecha de expicción, ademas de datos del usuario.
    /// Si no da errores 
    /// </returns>
    public async Task<Result<AuthResponseDto, DomainError>> LoginAsync(LoginDto request)
    {
        // Cargamos Usuario junto con Cliente y Trabajador para evitar NullReferenceException y comprobar IsActivo
        var user = await userManager.Users
            .Include(u => u.Cliente)
            .Include(u => u.Trabajador)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
        {
            logger.LogWarning("Credenciales invalidas del usuario");
            return Result.Failure<AuthResponseDto, DomainError>(AuthErrors.InvalidCredentials());
        }

        // Comprobamos que su ficha asociada siga activa (baja lógica)
        if ((user.Cliente is not null && !user.Cliente.IsActivo) ||
            (user.Trabajador is not null && !user.Trabajador.IsActivo))
        {
            logger.LogWarning("Intento de login de cuenta desactivada {UserId}", user.Id);
            return Result.Failure<AuthResponseDto, DomainError>(AuthErrors.InactiveAccount());
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Contraseña, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            logger.LogWarning("Contraseña incorrecta del usuario {UserId}", user.Id);
            return Result.Failure<AuthResponseDto, DomainError>(AuthErrors.InvalidCredentials());
        }

        var roles = await userManager.GetRolesAsync(user);
        var rolPrincipal = roles.FirstOrDefault() ?? Roles.Cliente;

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

        var tokenString = tokenHandler.WriteToken(token);

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

        return Result.Success<AuthResponseDto, DomainError>(new AuthResponseDto(tokenString, expiresAt, usuarioDto));
    }

    /// <summary>
    /// Se obtiene la información del perfil propio
    /// </summary>
    /// <param name="principal">Es el contexto de la seguridad</param>
    /// <returns>Devuelve los datos del usuario</returns>
    public async Task<Result<UsuarioResponseDto, DomainError>> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier) 
                        ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!long.TryParse(userIdStr, out var userId))
        {
            return Result.Failure<UsuarioResponseDto, DomainError>(AuthErrors.UserNotFound());
        }

        var user = await userManager.Users
            .Include(u => u.Cliente)
            .Include(u => u.Trabajador)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            logger.LogWarning("Usuario {UserId} no encontrado", userId);
            return Result.Failure<UsuarioResponseDto, DomainError>(AuthErrors.UserNotFound(userId));
        }

        var roles = await userManager.GetRolesAsync(user);
        var rolPrincipal = roles.FirstOrDefault() ?? Roles.Cliente;

        return Result.Success<UsuarioResponseDto, DomainError>(new UsuarioResponseDto(
            Id: user.Id,
            Email: user.Email ?? string.Empty,
            Telefono: user.PhoneNumber ?? string.Empty,
            Rol: rolPrincipal,
            ClienteId: user.Cliente?.Id,
            TrabajadorId: user.Trabajador?.Id,
            CreatedAt: user.CreatedAt
        ));
    }
    
}