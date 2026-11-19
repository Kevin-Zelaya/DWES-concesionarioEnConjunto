using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Usuarios;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace APIAlquilerVehiculos.Seed;

public static class UserRoleSeeder { 
    
    public static async Task SeedIdentityAsync(IServiceProvider services, ILogger logger) { 
        
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<long>>>();
        var userManager = services.GetRequiredService<UserManager<Usuario>>();
        var db = services.GetRequiredService<AppDbContext>();

        // crear roles si no existen
        await EnsureRolesExistAsync(roleManager, logger);

        // crear usuarios por defecto, sus entidades y la oficina
        await EnsureDefaultUsersAndOficinaExistAsync(userManager, db, logger);
    }

    private static async Task EnsureRolesExistAsync(RoleManager<IdentityRole<long>> roleManager, ILogger logger)
    {
        // recorre los roles a crear y los va creando
        foreach (var rol in Roles.Todos) {
            
            if (await roleManager.RoleExistsAsync(rol)) continue;
            
            await roleManager.CreateAsync(new IdentityRole<long>(rol));
            logger.LogInformation("Rol creado en Identity: {Rol}", rol);
        }
    }

    private static async Task EnsureDefaultUsersAndOficinaExistAsync(UserManager<Usuario> userManager, AppDbContext db, ILogger logger)
    {
        // admin
        await CreateUserIfNotExistsAsync(
            userManager, logger,
            email: "admin@alquiler.com",
            password: "Admin123@",
            telefono: "600000001",
            roles: [Roles.Administrador]
        );

        // responsable oficina
        var userResponsable = await CreateUserIfNotExistsAsync(
            userManager, logger,
            email: "responsable@alquiler.com",
            password: "Resp1234@",
            telefono: "600000002",
            roles: ["Operario", Roles.Responsable]
        );

        // si se ha creado, los asigno como responsable, si no lo encuentra creo uno
        Trabajador? trabajadorResponsable = null;
        if (userResponsable is not null)
        {
            trabajadorResponsable = await db.Trabajadores
                .FirstOrDefaultAsync(t => t.IdUsuario == userResponsable.Id);

            if (trabajadorResponsable is null)
            {
                trabajadorResponsable = new Trabajador
                {
                    IdUsuario = userResponsable.Id,
                    Nombre = "Carlos",
                    Apellidos = "García López",
                    Dni = "11111111H",
                    FechaIngreso = DateTime.UtcNow.AddYears(-3),
                    TipoTrabajador = TipoTrabajador.Operaciones,
                    IsActivo = true, 
                    CreatedAt = DateTime.UtcNow
                };
                db.Trabajadores.Add(trabajadorResponsable);
                await db.SaveChangesAsync();
            }
        }

        // operario
        var userOperario = await CreateUserIfNotExistsAsync(
            userManager, logger,
            email: "operario@alquiler.com",
            password: "Oper1234@",
            telefono: "600000003",
            roles: ["Operario"]
        );

        // si se ha creado bien lo mete a la bd
        if (userOperario is not null && !await db.Trabajadores.AnyAsync(t => t.IdUsuario == userOperario.Id))
        {
            db.Trabajadores.Add(new Trabajador
            {
                IdUsuario = userOperario.Id,
                Nombre = "Lucía",
                Apellidos = "Martínez Ruiz",
                Dni = "22222222J",
                FechaIngreso = DateTime.UtcNow.AddYears(-1),
                TipoTrabajador = TipoTrabajador.Operaciones,
                IsActivo = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // mecanico
        var userMecanico = await CreateUserIfNotExistsAsync(
            userManager, logger,
            email: "mecanico@alquiler.com",
            password: "Meca1234@",
            telefono: "600000004",
            roles: ["Mecanico"]
        );

        // si se ha creado bien lo mete a la bd
        if (userMecanico is not null && !await db.Trabajadores.AnyAsync(t => t.IdUsuario == userMecanico.Id))
        {
            db.Trabajadores.Add(new Trabajador
            {
                IdUsuario = userMecanico.Id,
                Nombre = "Miguel",
                Apellidos = "Sánchez Torres",
                Dni = "33333333P",
                FechaIngreso = DateTime.UtcNow.AddMonths(-6),
                TipoTrabajador = TipoTrabajador.Mecanico,
                IsActivo = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // cliente
        var userCliente = await CreateUserIfNotExistsAsync(
            userManager, logger,
            email: "cliente@alquiler.com",
            password: "Clie1234@",
            telefono: "600000005",
            roles: [Roles.Cliente]
        );

        // si se ha creado bien lo mete a la bd
        if (userCliente is not null && !await db.Clientes.AnyAsync(c => c.IdUsuario == userCliente.Id))
        {
            db.Clientes.Add(new Cliente
            {
                IdUsuario = userCliente.Id,
                Nombre = "Ana",
                Apellidos = "Fernández Pérez",
                FechaNacimiento = new DateTime(1995, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                Dni = "12345678Z",
                IsActivo = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // oficina apuntando al responsable
        if (!await db.Oficinas.AnyAsync() && trabajadorResponsable is not null)
        {
            db.Oficinas.Add(new Oficina
            {
                Name = "Oficina Central Leganés",
                Telefono = "910000000",
                Horario = "08:00-20:00",
                Direccion = new Direccion
                {
                    Calle = "Av. de la Universidad, 12",
                    CodigoPostal = "28911",
                    Ciudad = "Leganés"
                },
                Localizacion = new Localizacion
                {
                    Latitud = 40.327180m,
                    Longitud = -3.763500m
                },
                IdResponsable = trabajadorResponsable.Id,
                Activo = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Funcion auxiliar para crear los users
    /// </summary>
    /// <returns>Usuario creado</returns>
    private static async Task<Usuario?> CreateUserIfNotExistsAsync(
        UserManager<Usuario> userManager,
        ILogger logger,
        string email,
        string password,
        string telefono,
        string[] roles)
    {
        var usuarioExistente = await userManager.FindByEmailAsync(email);
        if (usuarioExistente is not null) return usuarioExistente; // si ya existe ese email lo devuelve

        var nuevoUsuario = new Usuario
        {
            UserName = email,
            Email = email,
            PhoneNumber = telefono,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true // evita bloqueos si la confirmación de email estuviese activa
        };

        var resultadoCreacion = await userManager.CreateAsync(nuevoUsuario, password);

        if (resultadoCreacion.Succeeded)
        {
            await userManager.AddToRolesAsync(nuevoUsuario, roles);
            logger.LogInformation("Usuario seed creado: {Email} con roles [{Roles}]", email, string.Join(", ", roles));
            return nuevoUsuario;
        }

        var errores = string.Join(" | ", resultadoCreacion.Errors.Select(e => e.Description));
        logger.LogError("Error al crear el usuario seed {Email}: {Errores}", email, errores);
        return null;
    }
}