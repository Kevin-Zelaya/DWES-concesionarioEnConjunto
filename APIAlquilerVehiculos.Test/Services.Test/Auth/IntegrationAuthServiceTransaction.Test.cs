using System.Data;
using APIAlquilerVehiculos.Dtos.Auth;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Services.Auth;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace APIAlquilerVehiculos.Test.Services.Test;

[TestFixture]
public class IntegrationAuthServiceTransactionTest
{
private PostgreSqlContainer _postgresContainer = null!;
    private string _connectionString = null!;

    private AppDbContext _dbContext = null!;
    private UserManager<Usuario> _userManager = null!;
    private Mock<SignInManager<Usuario>> _signInManagerMock = null!;
    private Mock<IValidator<SignupDto>> _validatorMock = null!;
    private Mock<ILogger<AuthService>> _loggerMock = null!;
    private Mock<IConfiguration> _configurationMock = null!;
    private AuthService _service = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("TestDb_Transactions")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _postgresContainer.StartAsync();
        _connectionString = _postgresContainer.GetConnectionString();

        // Aseguramos que el rol Cliente exista en la BD real de pruebas
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        await using var initContext = new AppDbContext(options);
        await initContext.Database.MigrateAsync();

        var roleStore = new RoleStore<IdentityRole<long>, AppDbContext, long>(initContext);
        using var roleManager = new RoleManager<IdentityRole<long>>(
            roleStore,
            Array.Empty<IRoleValidator<IdentityRole<long>>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new Mock<ILogger<RoleManager<IdentityRole<long>>>>().Object);

        if (!await roleManager.RoleExistsAsync(Roles.Cliente))
        {
            await roleManager.CreateAsync(new IdentityRole<long>(Roles.Cliente));
        }
    }

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        _dbContext = new AppDbContext(options);
        await _dbContext.Database.MigrateAsync();

        // UserManager REAL usando el mismo _dbContext (misma conexión y misma transacción)
        var userStore = new UserStore<Usuario, IdentityRole<long>, AppDbContext, long>(_dbContext);
        var identityOptions = Options.Create(new IdentityOptions
        {
            Password =
            {
                RequireDigit = true,
                RequiredLength = 6,
                RequireNonAlphanumeric = false,
                RequireUppercase = false,
                RequireLowercase = false
            }
        });

        _userManager = new UserManager<Usuario>(
            userStore,
            identityOptions,
            new PasswordHasher<Usuario>(),
            new IUserValidator<Usuario>[] { new UserValidator<Usuario>() },
            new IPasswordValidator<Usuario>[] { new PasswordValidator<Usuario>() },
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            services: null!,
            new Mock<ILogger<UserManager<Usuario>>>().Object);

        _validatorMock = new Mock<IValidator<SignupDto>>();
        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SignupDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<Usuario>>();
        _signInManagerMock = new Mock<SignInManager<Usuario>>(
            _userManager,
            contextAccessor.Object,
            claimsFactory.Object,
            identityOptions,
            new Mock<ILogger<SignInManager<Usuario>>>().Object,
            null!,
            null!);

        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _service = new AuthService(
            _validatorMock.Object,
            _userManager,
            _signInManagerMock.Object,
            _dbContext,
            _configurationMock.Object,
            _loggerMock.Object);
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_dbContext.Database.GetDbConnection().State == ConnectionState.Open)
        {
            await _dbContext.Clientes.ExecuteDeleteAsync();
            await _dbContext.UserRoles.ExecuteDeleteAsync();
            await _dbContext.Users.ExecuteDeleteAsync();
        }

        _userManager.Dispose();
        await _dbContext.DisposeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _postgresContainer.DisposeAsync();
    }

    #region helper

    private static SignupDto CrearDtoValido(string? dni = "12345678Z", string? email = null) => new(
        Email: email ?? $"user-{Guid.NewGuid()}@correo.com",
        Telefono: "600123456",
        Contraseña: "Password123!",
        Nombre: "Marcos",
        Apellidos: "Jodar",
        FechaNacimiento: new DateTime(1998, 4, 10, 0, 0, 0, DateTimeKind.Utc),
        Dni: dni,
        Nie: null,
        Pasaporte: null,
        NumeroPermisoConducir: "PERM12345"
    );

    private async Task<AppDbContext> CrearContextoIndependienteAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connectionString)
            .Options;
        return new AppDbContext(options);
    }

    #endregion

    // +++++++++++++++++++++++++++++++++++++++ ERRORES ++++++++++++++++++++++++++++++++++

    [Test]
    public async Task Signup_FallaSaveChangesAsync_DevuelveInternalError()
    {
        // Arrange: forzamos fallo al guardar el Cliente cerrando la conexión justo antes de SaveChangesAsync
        var email = $"savechanges-{Guid.NewGuid()}@correo.com";
        var request = CrearDtoValido(email: email);

        _dbContext.SavingChanges += async (_, _) =>
        {
            // Solo cerramos cuando ya se haya creado el usuario para simular fallo al guardar Cliente
            if (await _dbContext.Users.AnyAsync(u => u.Email == email))
            {
                await _dbContext.Database.CloseConnectionAsync();
            }
        };

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<InternalError>());
    }

    [Test]
    public async Task Signup_FallaSaveChangesAsync_SeEjecutaRollback()
    {
        // Arrange
        var email = $"rollback-save-{Guid.NewGuid()}@correo.com";
        var request = CrearDtoValido(email: email);
        var llamadasSaveChanges = 0;

        _dbContext.SavingChanges += (_, _) =>
        {
            llamadasSaveChanges++;

            // La 1.ª llamada guarda el Usuario; en la 2.ª (o cuando ya existe Cliente en el ChangeTracker) forzamos el fallo
            if (llamadasSaveChanges >= 2 || _dbContext.ChangeTracker.Entries<Cliente>().Any())
            {
                throw new InvalidOperationException("Fallo simulado en SaveChangesAsync al persistir Cliente");
            }
        };

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<InternalError>());
        Assert.That(_dbContext.Database.CurrentTransaction, Is.Null);

        await using var verifyCtx = await CrearContextoIndependienteAsync();
        Assert.That(await verifyCtx.Users.AnyAsync(u => u.Email == email), Is.False);
        Assert.That(await verifyCtx.Clientes.AnyAsync(), Is.False);
    }
    

    [Test]
    public async Task Signup_ExcepcionDuranteCreacionCliente_SeEjecutaRollback()
    {
        // Arrange: provocamos una excepción durante la persistencia del Cliente (ej. documento duplicado a nivel de BD o fallo de conexión)
        var email = $"ex-cliente-{Guid.NewGuid()}@correo.com";
        var request = CrearDtoValido(dni: "12345678Z", email: email);

        _dbContext.SavingChanges += (_, _) =>
        {
            throw new InvalidOperationException("Fallo simulado al crear la entidad Cliente");
        };

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(_dbContext.Database.CurrentTransaction, Is.Null);

        await using var verifyCtx = await CrearContextoIndependienteAsync();
        Assert.That(await verifyCtx.Users.AnyAsync(u => u.Email == email), Is.False);
        Assert.That(await verifyCtx.Clientes.AnyAsync(), Is.False);
    }

    [Test]
    public async Task Signup_ExcepcionDuranteRegistro_SeRegistraElErrorEnElLogger()
    {
        // Arrange
        var request = CrearDtoValido();

        _dbContext.SavingChanges += (_, _) =>
        {
            throw new Exception("Error inesperado en transacción de registro");
        };

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeastOnce);
    }

    [Test]
    public async Task Signup_ExcepcionDuranteRegistro_NoDevuelveResultadoDeExito()
    {
        // Arrange
        var request = CrearDtoValido();

        _dbContext.SavingChanges += (_, _) =>
        {
            throw new Exception("Fallo durante registro");
        };

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task Signup_ErrorAlCrearUsuario_NoQuedaUsuarioPersistido()
    {
        // Arrange: contraseña inválida según IdentityOptions (sin dígito) para que CreateAsync falle
        var email = $"fail-user-{Guid.NewGuid()}@correo.com";
        var request = CrearDtoValido(email: email) with { Contraseña = "PasswordSinNumero!" };

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);

        await using var verifyCtx = await CrearContextoIndependienteAsync();
        Assert.That(await verifyCtx.Users.AnyAsync(u => u.Email == email), Is.False);
        Assert.That(await verifyCtx.UserRoles.AnyAsync(), Is.False);
        Assert.That(await verifyCtx.Clientes.AnyAsync(), Is.False);
    }

    [Test]
    public async Task Signup_ErrorAlAsignarRol_NoQuedaRegistroParcialDelUsuario()
    {
        // Arrange: borramos temporalmente el rol Cliente para que AddToRoleAsync falle tras crear el Usuario
        var email = $"fail-role-{Guid.NewGuid()}@correo.com";
        var request = CrearDtoValido(email: email);

        var rolCliente = await _dbContext.Roles.FirstAsync(r => r.Name == Roles.Cliente);
        _dbContext.Roles.Remove(rolCliente);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.SignupAsync(request);

        // Reponemos el rol para el resto de pruebas
        await using var restoreCtx = await CrearContextoIndependienteAsync();
        if (!await restoreCtx.Roles.AnyAsync(r => r.Name == Roles.Cliente))
        {
            restoreCtx.Roles.Add(new IdentityRole<long>
            {
                Name = Roles.Cliente,
                NormalizedName = Roles.Cliente.ToUpperInvariant()
            });
            await restoreCtx.SaveChangesAsync();
        }

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(_dbContext.Database.CurrentTransaction, Is.Null);

        await using var verifyCtx = await CrearContextoIndependienteAsync();
        Assert.That(await verifyCtx.Users.AnyAsync(u => u.Email == email), Is.False);
        Assert.That(await verifyCtx.Clientes.AnyAsync(), Is.False);
    }

    [Test]
    public async Task Signup_ErrorAlGuardarCliente_NoQuedaRegistroParcialDelUsuarioYCliente()
    {
        // Arrange: provocamos fallo justo al persistir Cliente después de crear Usuario y asignar rol
        var email = $"fail-cliente-{Guid.NewGuid()}@correo.com";
        var request = CrearDtoValido(email: email);

        _dbContext.SavingChanges += (_, _) =>
        {
            if (_dbContext.ChangeTracker.Entries<Cliente>().Any(e => e.State == EntityState.Added))
            {
                throw new DbUpdateException("Error simulado al insertar Cliente");
            }
        };

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(_dbContext.Database.CurrentTransaction, Is.Null);

        await using var verifyCtx = await CrearContextoIndependienteAsync();
        Assert.That(await verifyCtx.Users.AnyAsync(u => u.Email == email), Is.False);
        Assert.That(await verifyCtx.UserRoles.AnyAsync(), Is.False);
        Assert.That(await verifyCtx.Clientes.AnyAsync(), Is.False);
    }  
}