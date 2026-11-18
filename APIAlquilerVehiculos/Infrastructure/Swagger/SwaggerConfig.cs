using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Filters;

namespace APIAlquilerVehiculos.Infrastructure.Swagger;

/// <summary>
/// Configuración de Swagger/OpenAPI.
/// </summary>
public static class SwaggerConfig
{
    /// <summary>
    ///  Configuracón Swagger/OpenAPI con documentación completa y seguridad JWT.
    /// </summary>
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1.0.0",
                Title = "Alquiler de Vehiculos API",
                Description = "API para gestionar vehículos, usuarios y alquileres.",
                Contact = new OpenApiContact
                {
                    Name = "Marcos Jodar Gómez\n" +
                           "Demas",
                    Email = "X"
                }
            });
            
            // Definir esquema de seguridad JWT
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Ingrese 'Bearer' seguido de un espacio y el token JWT."
            });
            
            // Aplicar esquema de seguridad globalmente
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", document),
                    new List<string>() // Soluciona el error de asignación de tipo al requerir List<string>
                }
            });

            // Incluir comentarios XML (solo si existe: si no, IncludeXmlComments lanza FileNotFoundException)
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            options.EnableAnnotations();
            //options.ExampleFilters();
            
        });
        //services.AddSwaggerExamplesFromAssemblyOf<SignupRequestExample>(); --> Para ejemplos (en un futuro)
        return services;
    }
}