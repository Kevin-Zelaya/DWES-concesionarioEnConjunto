using System.Text;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace APIAlquilerVehiculos;

public static class AutenticacionConfig
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // AddIdentityCore: Identity sin UI; roles y signInManager se añaden a mano
       services.AddIdentityCore<Usuario>(options =>
            {
                // --- CONTRASEÑA ---
                options.Password.RequireDigit = true;           // Exige al menos un número (0-9).
                options.Password.RequireLowercase = true;       // Exige al menos una minúscula (a-z).
                options.Password.RequireUppercase = true;       // Exige al menos una mayúscula (A-Z).
                options.Password.RequireNonAlphanumeric = true; // Exige al menos un carácter especial (!, ?, @, #...).
                options.Password.RequiredLength = 12;            // Establece el mínimo de caracteres (8)

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
        return services;
    }
}