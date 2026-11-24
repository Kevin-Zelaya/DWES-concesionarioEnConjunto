using System.Text;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Usuarios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace APIAlquilerVehiculos.Infrastructure.AutenticationJWTAutorization;

/// <summary>
///  Configuración de autenticación y autorización.
///  Uso de ASP.NET Core Identity (UserManager/SignInManager) + JWT Bearer
///  Devuelve un JWT Bearer que los endpoints validan.
/// </summary>
public static class AutenticacionConfig
{
    public static IServiceCollection AddAuthenticationConfing(this IServiceCollection services, IConfiguration configuration)
    {
        // AddIdentityCore: Identity sin UI; roles y signInManager se añaden a mano
       services.AddIdentityCore<Usuario>(options =>
            {
                // --- CONTRASEÑA ---
                options.Password.RequireDigit = true;           // Exige al menos un número (0-9).
                options.Password.RequireLowercase = true;       // Exige al menos una minúscula (a-z).
                options.Password.RequireUppercase = true;       // Exige al menos una mayúscula (A-Z).
                options.Password.RequireNonAlphanumeric = true; // Exige al menos un carácter especial (!, ?, @, #...).
                options.Password.RequiredLength = 8;            // Establece el mínimo de caracteres (8)

                // --- USUARIO ---
                options.User.RequireUniqueEmail = true;         // Email único

                // --- CUENTA ---
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Tiempo de bloqueo (15 minutos)
                options.Lockout.MaxFailedAccessAttempts = 5;    // Intentos fallidos permitidos antes del bloqueo (5)
            })
            .AddRoles<IdentityRole<long>>()
            .AddSignInManager()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Forzar JWT Bearer como esquema global evitando conflictos con cookies
        //Lo fijamos a Bearer para que ningún registro futuro pueda cambiar el comportamiento.
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;                 //
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;     //
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;        //
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;           //
            })
            .AddJwtBearer(options =>
            {
                // Desactivado el mapeo automático de claims
                options.MapInboundClaims = false;
                
                #region Validacion del Token

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Validar que el token haya sido firmado con nuestra clave secreta
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret no configurado"))),
                    
                    // Validar quién emite el token (Issuer)
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    
                    // Validar para quién va dirigido el token (Audience)
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                   
                    //Validar que el token no haya expirado
                    ValidateLifetime = true,
                    
                    // Evita desfases horarios (da un margen de unos pocos segundos de tolerancia)
                    ClockSkew = TimeSpan.Zero,
                    
                    //Indicamos qué claim del JWT representa los roles
                    RoleClaimType = "role"
                };
                #endregion
            });
        
        AddAuthorizationPolicies(services);
        return services;
    }
    
    /// <summary>
    ///  Políticas de autorización.
    /// </summary>
    private static void AddAuthorizationPolicies(IServiceCollection services)
    {
        //Log.Information("🛡️ Configurando políticas de autorización...");
        // Todas las políticas declaran el esquema Bearer EXPLÍCITAMENTE.
        // Al declarar el esquema aquí inmuniza las políticas ante cualquier cambio futuro.
        // Se seguira autenticando con JWT en vez de redirigir a /Account/Login.
        // Es la buena práctica en cualquier API JWT: la política es autosuficiente.
        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build())
            // Solo Administrador (catálogos globales, gestión de usuarios, etc.)
            .AddPolicy("RequireAdminRole", new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireRole(Roles.Administrador)
                .Build())
            // Acciones solo del Cliente (dejar valoraciones o Gestionar su cuenta)
            .AddPolicy("RequireClienteRole", new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireRole(Roles.Cliente)
                .Build())
            // Acciones del Cliente y del Administrador (Gestionar clientes)
            .AddPolicy("RequireClienteYAdminRole", new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireRole(Roles.Administrador, Roles.Cliente)
                .Build())
            // Para la mayoria de enspoints que se requiera al Tabajador sea del tipo que sea.
            .AddPolicy("RequireTrabajadorRole", new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireRole(Roles.Mecanico, Roles.Operario)
                .Build())
            //Para abrir y cerrar partes de taller
            .AddPolicy("RequireMecanicoRole", new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireRole(Roles.Mecanico, Roles.Responsable)
                .Build())
            // Acciones del responsable
            .AddPolicy("RequireResponsableRole", new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                .RequireRole(Roles.Responsable, Roles.Administrador)
                .Build());
        //Para acciones generales que requieran autorizacion simplemente [Authorize]
        //Para acciones publicas sin autorization.
    }
}