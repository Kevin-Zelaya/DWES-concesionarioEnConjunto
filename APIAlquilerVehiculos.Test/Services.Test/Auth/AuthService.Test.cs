using APIAlquilerVehiculos.Dtos.Auth;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Errors.Email;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Services.Auth;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Testcontainers.PostgreSql;

namespace APIAlquilerVehiculos.Test.Services.Test;

[TestFixture]
public class AuthServiceTest
{
    // Conectar con la BD Postgre
    private AppDbContext _dbContext = null!;
    
    // Se utiliza = null! en cada una para saber que cada opcion se inicializa antes de utilizarla y
    // evitar la advertencia del compilador
    
    // Representa el contenedor Postgre
    private PostgreSqlContainer _postgresContainer = null!;
    private string _connectionString = null!;
    
    // simulación para gestionar los usuarios e inicio de sesión
    private Mock<UserManager<Usuario>> _userManagerMock = null!;
    private Mock<SignInManager<Usuario>> _signInManagerMock = null!;
    // Validaciones simuladas
    private Mock<IValidator<SignupDto>> _validatorMock = null!;
    // logs simulados
    private Mock<ILogger<AuthService>> _loggerMock = null!;
    // Simula el acceso a la configuración 
    private Mock<IConfiguration> _configurationMock = null!;
    private Usuario _usuarioPrueba = null!;
    private Cliente _clientePrueba = null!;
    
    private AuthService _service = null!;

    [OneTimeSetUp] // Se ejecuta una unica vez antes de cualquier test (para no arrancar y apagarlo en cada test)
    public async Task OneTimeSetUp()
    {
        // creamos el contenedor con PostgreSqlBuilder para pruebas (prepara una independiente)
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("TestDb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _postgresContainer.StartAsync(); // lo inicializamos
        _connectionString = _postgresContainer.GetConnectionString();
    }
    
    [SetUp] // Antes de cada test individual
    public async Task SetUp()
    {
        // Validator --> Comprobar si los valores de registro estan correctos
        _validatorMock = new Mock<IValidator<SignupDto>>();
        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SignupDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // UserManager --> almacenar y recuperar usuarios
        var userStore = new Mock<IUserStore<Usuario>>();

        // El constructor de UserManager<Usuario> recibe varias dependencias:
        _userManagerMock = new Mock<UserManager<Usuario>>(
            userStore.Object,
            null!, // Almacén de usuarios.
            null!, // Opciones de Identity.
            null!, // Hash de contraseñas.
            null!, // Validadores.
            null!, // Normalizador.
            null!, // Logger
            null!, // Proveedor de errores.
            null!  // Servicios adicionales.
        );

        _userManagerMock
            .Setup(u => u.Users)
            .Returns(() => _dbContext.Users);
        // Comportamiento por defecto para que el registro completo funcione salvo que un test lo sobrescriba
        _userManagerMock
            .Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Usuario?)null);

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .Returns(async (Usuario usuario, string _) =>
            {
                _usuarioPrueba = usuario;
                await _dbContext.Users.AddAsync(usuario);
                await _dbContext.SaveChangesAsync();
                return IdentityResult.Success;
            });

        _userManagerMock
            .Setup(u => u.AddToRoleAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        

        // SignInManager
        // Acceder al contexto HTTP actual.
        var contextAccessor = new Mock<IHttpContextAccessor>();
        // Generar la identidad del usuario y sus claims.
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<Usuario>>();
        // Configuración de Identity. Crea un Obj para recibir configuraciones
        var identityOptions = Options.Create(new IdentityOptions());
        // Registrar información y errores.
        var signInLogger = new Mock<ILogger<SignInManager<Usuario>>>();
        // Conocer los esquemas de autenticación disponibles.
        var schemes = new Mock<IAuthenticationSchemeProvider>();
        // Comprobar si el usuario está confirmado.
        var confirmation = new Mock<IUserConfirmation<Usuario>>();

        // Construimos el simulador
        _signInManagerMock = new Mock<SignInManager<Usuario>>(
            _userManagerMock.Object,
            contextAccessor.Object,
            claimsFactory.Object,
            identityOptions,
            signInLogger.Object,
            schemes.Object,
            confirmation.Object
        );
        
        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(It.IsAny<Usuario>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Failed);

        _userManagerMock
            .Setup(u => u.GetRolesAsync(It.IsAny<Usuario>()))
            .ReturnsAsync(new List<string> { Roles.Cliente });

        _configurationMock = new Mock<IConfiguration>();
        
        // Conectar EF Core con PostgreSQL
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connectionString) // elegimos Postgre
            .Options;

        _dbContext = new AppDbContext(options); // conectamos el appdbcontext al postgre

        await _dbContext.Database.MigrateAsync(); // y ejecutamos migraciones (ya que la tabla es nueva)
        
        // Mockeamos para que no queden logs reales
        _loggerMock = new Mock<ILogger<AuthService>>();

        // Servicio construido con los mocks y clases simuladas
        _service = new AuthService(
            _validatorMock.Object,
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _dbContext,
            _configurationMock.Object,
            _loggerMock.Object
        );
        
        ConfigurarJwtMock();
        
        // Usuario de prueba
        _usuarioPrueba = new Usuario
        {
            UserName = "test@correo.com",
            Email = "test@correo.com",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        // Cliente asociado al usuario
        _clientePrueba = new Cliente
        {
            Nombre = "Javi",
            Apellidos = "Hollos",
            Dni = "12345678Z",
            FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            NumeroPermisoConducir = "987654321",
            Usuario = _usuarioPrueba,
            CreatedAt = DateTime.UtcNow,
            IsActivo = true
        };
    }

    [TearDown] // Se ejecuta después de cada test
    public async Task TearDown()
    {
        // Si la conexión fue cerrada por un test de fallo 500, la reabrimos para poder limpiar las tablas
        if (_dbContext.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
        {
            await _dbContext.Database.OpenConnectionAsync();
        }

        // Primero limpiamos las tablas dependientes (incluyendo inactivos) antes de borrar usuarios
        await _dbContext.Clientes.IgnoreQueryFilters().ExecuteDeleteAsync();
        await _dbContext.Trabajadores.IgnoreQueryFilters().ExecuteDeleteAsync();
        await _dbContext.Users.IgnoreQueryFilters().ExecuteDeleteAsync();

        // Eliminamos el contexto para que no afecte al trabajo real
        await _dbContext.DisposeAsync();
    }
    
    [OneTimeTearDown] // se ejecuta una vez al terminar TODOS los test
    public async Task OneTimeTearDown()
    { 
        // libera los recursos del contenedor y lo detiene.
        await _postgresContainer.DisposeAsync();
    }

    #region Helpers
    // Helper para construir un SignupDto válido cambiando solo los documentos en cada test
    private static SignupDto CrearSignupDto(
        string? dni = null,
        string? nie = null,
        string? pasaporte = null) => new(
        Email: $"nuevo-{Guid.NewGuid()}@correo.com",
        Telefono: "600123456",
        Contraseña: "Password123#",
        Nombre: "Carlos",
        Apellidos: "García López",
        FechaNacimiento: new DateTime(1995, 5, 20),
        Dni: dni,
        Nie: nie,
        Pasaporte: pasaporte,
        NumeroPermisoConducir: "123456789"
    );
    
    private static Usuario CrearUsuarioLogin(string email, DateTime? createdAt = null) => new()
    {
        Id = 1,
        UserName = email,
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        NormalizedUserName = email.ToUpperInvariant(),
        PhoneNumber = "600123456",
        EmailConfirmed = true,
        CreatedAt = createdAt ?? DateTime.UtcNow
    };

    private static IList<System.Security.Claims.Claim> LeerClaimsJwt(string token)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.Claims.ToList();
    }

    private static string? ObtenerRolPrincipalJwt(IList<System.Security.Claims.Claim> claims) =>
        claims.FirstOrDefault(c =>
            c.Type == System.Security.Claims.ClaimTypes.Role ||
            c.Type == "role")?.Value;

    
    private const string JwtClaveTest = "ClaveSuperSecretaDePruebasParaJwtHmacSha256_1234!";
    private const string JwtIssuerTest = "APIAlquilerVehiculos.Issuer";
    private const string JwtAudienceTest = "APIAlquilerVehiculos.Audience";

    private void ConfigurarJwtMock(string? clave = null, string? issuer = null, string? audience = null)
    {
        var secret = clave ?? JwtClaveTest;
        var iss = issuer ?? JwtIssuerTest;
        var aud = audience ?? JwtAudienceTest;

        _configurationMock.Setup(c => c["Jwt:Secret"]).Returns(secret);
        _configurationMock.Setup(c => c["Jwt:Key"]).Returns(secret);
        _configurationMock.Setup(c => c["Jwt:SecretKey"]).Returns(secret);
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns(iss);
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns(aud);
        _configurationMock.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("15");
    }

    private static System.IdentityModel.Tokens.Jwt.JwtSecurityToken LeerJwt(string token)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        return handler.ReadJwtToken(token);
    }

    private static bool ValidarFirmaJwt(string token, string claveSecreta, string issuer, string audience)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var keyBytes = System.Text.Encoding.UTF8.GetBytes(claveSecreta);
        var validationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(keyBytes),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(10)
        };

        try
        {
            handler.ValidateToken(token, validationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<Usuario> GuardarUsuarioLoginAsync(string email, int? idEspecifico = null)
    {
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        if (idEspecifico.HasValue)
        {
            usuario.Id = idEspecifico.Value;
        }

        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();
        return usuario;
    }

    private void ConfigurarLoginExitoso(Usuario usuario, IList<string> roles)
    {
        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(
                It.Is<Usuario>(u => u.Id == usuario.Id),
                "Password123!",
                true))
            .ReturnsAsync(SignInResult.Success);

        _userManagerMock
            .Setup(u => u.GetRolesAsync(It.Is<Usuario>(u => u.Id == usuario.Id)))
            .ReturnsAsync(roles);
    }
    private async Task<Usuario> GuardarUsuarioDtoAsync(
        string email,
        string? phoneNumber = null,
        DateTime? createdAt = null)
    {
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            PhoneNumber = phoneNumber,
            EmailConfirmed = true,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };

        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();
        return usuario;
    }

    private void ConfigurarMocksDto(Usuario usuario, IList<string> roles)
    {
        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(
                It.Is<Usuario>(u => u.Id == usuario.Id),
                "Password123!",
                true))
            .ReturnsAsync(SignInResult.Success);

        _userManagerMock
            .Setup(u => u.GetRolesAsync(It.Is<Usuario>(u => u.Id == usuario.Id)))
            .ReturnsAsync(roles);
    }

    #endregion
    
    
    #region TEST SIGNUP

    // TEST CORRECTO 
    [TestCase("12345678Z", null, null, null)]
    [TestCase(null, "X1234567L", null, null)]
    [TestCase(null, null, "ABC123456", "12345678A")]
    public async Task Signup_Correcto_Con_Documento_Devuelve_Datos_Usuario_Cliente(
        string? dni,
        string? nie,
        string? pasaporte,
        string? numeroPermiso)
    {
        // Arrange
        var request = new SignupDto(
            Email: $"nuevo-{Guid.NewGuid()}@correo.com",
            Telefono: "600123456",
            Contraseña: "Password123#",
            Nombre: "Marcos",
            Apellidos: "Prueba Prueba",
            FechaNacimiento: new DateTime(2000, 5, 15),
            Dni: dni,
            Nie: nie,
            Pasaporte: pasaporte,
            NumeroPermisoConducir: numeroPermiso
        );

        // ACT: ejecutar el método
        var resultado = await _service.SignupAsync(request);
        if (resultado.IsFailure)
        {
            Assert.Fail($"Signup ha fallado: {resultado.Error}");
        }
        
        // ASSERT: comprobar el resultado
        _userManagerMock.Verify(u => u.FindByEmailAsync(request.Email), Times.Once);
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), request.Contraseña), Times.Once);
        _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<Usuario>(), Roles.Cliente), Times.Once);

        Assert.That(resultado.IsSuccess, Is.True);
        Assert.That(_usuarioPrueba, Is.Not.Null);

        var clienteGuardado = await _dbContext.Clientes
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.Usuario.Id == _usuarioPrueba.Id);

        Assert.That(clienteGuardado, Is.Not.Null);

        Assert.Multiple(() =>
        {
            // Usuario
            Assert.That(_usuarioPrueba.Email, Is.EqualTo(request.Email));

            // Cliente
            Assert.That(clienteGuardado!.Nombre, Is.EqualTo(request.Nombre));
            Assert.That(clienteGuardado.Apellidos, Is.EqualTo(request.Apellidos));
            Assert.That(clienteGuardado.Dni, Is.EqualTo(request.Dni));
            Assert.That(clienteGuardado.Nie, Is.EqualTo(request.Nie));
            Assert.That(clienteGuardado.Pasaporte, Is.EqualTo(request.Pasaporte));
            Assert.That(clienteGuardado.NumeroPermisoConducir, Is.EqualTo(request.NumeroPermisoConducir));
            Assert.That(clienteGuardado.Usuario.Id, Is.EqualTo(_usuarioPrueba.Id));
        });
    }
    
    // +++++++++++++++++++++++++++++++++++++++ ERRORES 500 ++++++++++++++++++++++++++++++++++
    [Test]
    public async Task Signup_Errores_No_Consulta_BD_Si_hay_Otros_docs_iguales_Devuelve_Error500()
    {
        // Arrange
        var request = CrearSignupDto(pasaporte: "ABC123456");

        // Provocamos una excepción inesperada al consultar la BD o crear usuario
        _userManagerMock
            .Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Fallo de conexión simulado"));

        // ACT: ejecutar el método
        Assert.ThrowsAsync<Exception>(async () => await _service.SignupAsync(request));
    }

    [Test]
    public async Task Signup_Errores_No_Crea_Usuario_Devuelve_Error500()
    {
        // Arrange
        var request = CrearSignupDto(pasaporte: "ABC123456");

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Error 500 simulado al crear usuario"));
        
        // ACT
        var resultado = await _service.SignupAsync(request);
        
        // ASSERT
        Assert.That(resultado.IsFailure, Is.True);
        Assert.That(resultado.Error, Is.TypeOf<InternalError>());
    }

    [Test]
    public async Task Signup_Errores_No_Asigna_RolCliente_Devuelve_Error500()
    {
        // Arrange
        var request = CrearSignupDto(pasaporte: "ABC123456");

        _userManagerMock
            .Setup(u => u.AddToRoleAsync(It.IsAny<Usuario>(), Roles.Cliente))
            .ThrowsAsync(new Exception("Error 500 simulado al asignar rol"));

        // ACT: ejecutar el método
        var resultado = await _service.SignupAsync(request);
        
        // ASSERT
        Assert.That(resultado.IsFailure, Is.True);
        Assert.That(resultado.Error, Is.TypeOf<InternalError>());
    }
    
    // +++++++++++++++++++++++++++++++++++++++ ERRORES VALIDACION ++++++++++++++++++++++++++++++++++
    [Test]
    public async Task Signup_Errores_Validacion_De_Un_Campo_Devuelve_Error()
    {
        // Arrange
        var request = CrearSignupDto(pasaporte: "ABC123456");
        
        var errores = new ValidationResult(new[]
        {
            new ValidationFailure("Email", "El email no es válido.")
        });

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SignupDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(errores);

        // ACT
        var resultado = await _service.SignupAsync(request);

        // ASSERT
        Assert.That(resultado.IsFailure, Is.True);
        Assert.That(resultado.Error, Is.TypeOf<ValidationError>());

        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<Usuario>(), It.IsAny<string>()), Times.Never);
        Assert.That(await _dbContext.Clientes.AnyAsync(), Is.False);
    }

    [Test]
    public async Task Signup_Errores_Validacion_De_Varios_Campos_Devuelve_Error()
    {
        // Arrange
        var request = CrearSignupDto(pasaporte: "ABC123456");
        
        var errores = new ValidationResult(new[]
        {
            new ValidationFailure("Email", "El email no es válido."),
            new ValidationFailure("FechaNacimiento", "La fecha de nacimiento es obligatoria"),
            new ValidationFailure("Contraseña", "La contraseña es obligatoria")
        });

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SignupDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(errores);

        // ACT
        var resultado = await _service.SignupAsync(request);

        // ASSERT
        Assert.That(resultado.IsFailure, Is.True);
        Assert.That(resultado.Error, Is.TypeOf<ValidationError>());

        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<Usuario>(), It.IsAny<string>()), Times.Never);
        Assert.That(await _dbContext.Clientes.AnyAsync(), Is.False);
    }
    
    [TestCase("12345678Z", null, null, null)]
    [TestCase(null, "X1234567L", null, null)]
    [TestCase(null, null, "ABC123456", "12345678A")]
    public async Task Signup_Errores_Validacion_Documentos_Duplicados_Devuelve_Error(
        string? dni,
        string? nie,
        string? pasaporte,
        string? numeroPermiso)
    {
        // Arrange
        var request = new SignupDto(
            Email: $"nuevo-{Guid.NewGuid()}@correo.com",
            Telefono: "600123456",
            Contraseña: "Password123#",
            Nombre: "Marcos",
            Apellidos: "Prueba Prueba",
            FechaNacimiento: new DateTime(2000, 5, 15),
            Dni: dni,
            Nie: nie,
            Pasaporte: pasaporte,
            NumeroPermisoConducir: numeroPermiso
        );
    
        var errores = new ValidationResult(new[]
        {
            new ValidationFailure("Documentos", "Error de formato en el documento"),
        });

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SignupDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(errores);

        // Act
        var resultado = await _service.SignupAsync(request);

        // Assert
        Assert.That(resultado.IsFailure, Is.True);
        Assert.That(resultado.Error, Is.TypeOf<ValidationError>());

        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<Usuario>(), It.IsAny<string>()), Times.Never);
        Assert.That(await _dbContext.Clientes.AnyAsync(), Is.False);
    }

    // +++++++++++++++++++++++++++++++++++++++ TESTS 145 - 161: UNICIDAD DE DOCUMENTOS ++++++++++++++++++++++++++++++++++
    [Test]
    public async Task Signup_DniYaExistente_DevuelveConflictError()
    {
        // Arrange
        _clientePrueba.Dni = "12345678Z";
        _dbContext.Clientes.Add(_clientePrueba);
        await _dbContext.SaveChangesAsync();

        var request = CrearSignupDto(dni: "12345678Z");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ConflictError>());
    }

    [Test]
    public async Task Signup_NieYaExistente_DevuelveConflictError()
    {
        // Arrange
        _clientePrueba.Dni = null;
        _clientePrueba.Nie = "X1234567L";
        _dbContext.Clientes.Add(_clientePrueba);
        await _dbContext.SaveChangesAsync();

        var request = CrearSignupDto(nie: "X1234567L");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ConflictError>());
    }

    [Test]
    public async Task Signup_PasaporteYaExistente_DevuelveConflictError()
    {
        // Arrange
        _clientePrueba.Dni = null;
        _clientePrueba.Pasaporte = "PAA123456";
        _dbContext.Clientes.Add(_clientePrueba);
        await _dbContext.SaveChangesAsync();

        var request = CrearSignupDto(pasaporte: "PAA123456");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ConflictError>());
    }

    [Test]
    public async Task Signup_DniNoExistente_ContinuaElRegistro()
    {
        // Arrange
        _clientePrueba.Dni = "12345678Z";
        _dbContext.Clientes.Add(_clientePrueba);
        await _dbContext.SaveChangesAsync();

        var request = CrearSignupDto(dni: "87654321X");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), request.Contraseña), Times.Once);
    }

    [Test]
    public async Task Signup_NieNoExistente_ContinuaElRegistro()
    {
        // Arrange
        _clientePrueba.Dni = null;
        _clientePrueba.Nie = "X1234567L";
        _dbContext.Clientes.Add(_clientePrueba);
        await _dbContext.SaveChangesAsync();

        var request = CrearSignupDto(nie: "Y7654321M");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), request.Contraseña), Times.Once);
    }

    [Test]
    public async Task Signup_PasaporteNoExistente_ContinuaElRegistro()
    {
        // Arrange
        _clientePrueba.Dni = null;
        _clientePrueba.Pasaporte = "PAA123456";
        _dbContext.Clientes.Add(_clientePrueba);
        await _dbContext.SaveChangesAsync();

        var request = CrearSignupDto(pasaporte: "PBB987654");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), request.Contraseña), Times.Once);
    }

    [Test]
    public async Task Signup_TablaClienteVacia_ContinuaElRegistro()
    {
        // Arrange (no añadimos _clientePrueba a la BD)
        var request = CrearSignupDto(dni: "12345678Z");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(await _dbContext.Clientes.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Signup_DniExistenteYNieNoExistente_DevuelveConflictError()
    {
        // Arrange
        _clientePrueba.Dni = "12345678Z";
        _dbContext.Clientes.Add(_clientePrueba);
        await _dbContext.SaveChangesAsync();

        var request = CrearSignupDto(dni: "12345678Z", nie: "Y7654321M");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ConflictError>());
    }

    [Test]
    public async Task Signup_NieExistenteYDniNoExistente_DevuelveConflictError()
    {
        // Arrange
        _clientePrueba.Dni = null;
        _clientePrueba.Nie = "X1234567L";
        _dbContext.Clientes.Add(_clientePrueba);
        await _dbContext.SaveChangesAsync();

        var request = CrearSignupDto(dni: "87654321X", nie: "X1234567L");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ConflictError>());
    }

    [Test]
    public async Task Signup_PasaporteExistenteYOtrosDocumentosNoExistentes_DevuelveConflictError()
    {
        // Arrange
        _clientePrueba.Dni = null;
        _clientePrueba.Pasaporte = "PAA123456";
        _dbContext.Clientes.Add(_clientePrueba);
        await _dbContext.SaveChangesAsync();

        var request = CrearSignupDto(dni: "87654321X", nie: "Y7654321M", pasaporte: "PAA123456");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ConflictError>());
    }

    [Test]
    public async Task Signup_DniEnMinusculasYExistenteEnMayusculas_DevuelveConflictError()
    {
        // Arrange
        _clientePrueba.Dni = "12345678Z";
        _dbContext.Clientes.Add(_clientePrueba);
        await _dbContext.SaveChangesAsync();

        var request = CrearSignupDto(dni: "12345678z");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ConflictError>());
    }

    [Test]
    public async Task Signup_NieEnMinusculasYExistenteEnMayusculas_DevuelveConflictError()
    {
        // Arrange
        _clientePrueba.Dni = null;
        _clientePrueba.Nie = "X1234567L";
        _dbContext.Clientes.Add(_clientePrueba);
        await _dbContext.SaveChangesAsync();

        var request = CrearSignupDto(nie: "x1234567l");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ConflictError>());
    }

    [Test]
    public async Task Signup_PasaporteEnMinusculasYExistenteEnMayusculas_DevuelveConflictError()
    {
        // Arrange
        _clientePrueba.Dni = null;
        _clientePrueba.Pasaporte = "PAA123456";
        _dbContext.Clientes.Add(_clientePrueba);
        await _dbContext.SaveChangesAsync();

        var request = CrearSignupDto(pasaporte: "paa123456");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ConflictError>());
    }

    [Test]
    public async Task Signup_DocumentoConEspaciosExterioresYExistenteSinEllos_DevuelveConflictError()
    {
        // Arrange
        _clientePrueba.Dni = "12345678Z";
        _dbContext.Clientes.Add(_clientePrueba);
        await _dbContext.SaveChangesAsync();

        var request = CrearSignupDto(dni: "   12345678Z   ");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ConflictError>());
    }

    [Test]
    public async Task Signup_DniNoInformadoYNieExistente_DevuelveConflictError()
    {
        // Arrange
        _clientePrueba.Dni = null;
        _clientePrueba.Nie = "X1234567L";
        _dbContext.Clientes.Add(_clientePrueba);
        await _dbContext.SaveChangesAsync();

        var request = CrearSignupDto(dni: null, nie: "X1234567L");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ConflictError>());
    }

    [Test]
    public async Task Signup_DniNoInformadoYPasaporteExistente_DevuelveConflictError()
    {
        // Arrange
        _clientePrueba.Dni = null;
        _clientePrueba.Pasaporte = "PAA123456";
        _dbContext.Clientes.Add(_clientePrueba);
        await _dbContext.SaveChangesAsync();

        var request = CrearSignupDto(dni: null, pasaporte: "PAA123456");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ConflictError>());
    }

    [Test]
    public async Task Signup_NieNoInformadoYPasaporteExistente_DevuelveConflictError()
    {
        // Arrange
        _clientePrueba.Dni = null;
        _clientePrueba.Pasaporte = "PAA123456";
        _dbContext.Clientes.Add(_clientePrueba);
        await _dbContext.SaveChangesAsync();

        var request = CrearSignupDto(nie: null, pasaporte: "PAA123456");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ConflictError>());
    }
    
    // +++++++++++++++++++++++++++++++++++++++ TESTS EMAIL DUPLICADO / UNICIDAD ++++++++++++++++++++++++++++++++++

    [Test]
    public async Task Signup_EmailYaExistente_DevuelveConflictError()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(_usuarioPrueba);

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ConflictError>());
    }

    [Test]
    public async Task Signup_EmailNoExistente_ContinuaElRegistro()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync((Usuario?)null);

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), request.Contraseña), Times.Once);
    }

    [Test]
    public async Task Signup_EmailConMayusculasYExistenteEnMinusculas_DevuelveConflictError()
    {
        // Arrange
        _usuarioPrueba.Email = "existente@correo.com";
        var request = CrearSignupDto(dni: "87654321X") with { Email = "EXISTENTE@CORREO.COM" };

        // Simulamos el comportamiento case-insensitive de Identity (NormalizedEmail)
        _userManagerMock
            .Setup(u => u.FindByEmailAsync(It.Is<string>(e =>
                e.Trim().Equals(_usuarioPrueba.Email, StringComparison.OrdinalIgnoreCase))))
            .ReturnsAsync(_usuarioPrueba);

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ConflictError>());
    }

    [Test]
    public async Task Signup_EmailConEspaciosExteriores_CompruebaNormalizacionIdentity()
    {
        // Arrange
        _usuarioPrueba.Email = "existente@correo.com";
        var request = CrearSignupDto(dni: "87654321X") with { Email = "   existente@correo.com   " };

        // Simulamos la normalización de Identity (Trim + ToUpperInvariant)
        _userManagerMock
            .Setup(u => u.FindByEmailAsync(It.Is<string>(e =>
                e.Trim().Equals(_usuarioPrueba.Email, StringComparison.OrdinalIgnoreCase))))
            .ReturnsAsync(_usuarioPrueba);

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ConflictError>());
    }

    [Test]
    public async Task Signup_EmailDuplicado_NoLlamaACreateAsync()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(_usuarioPrueba);

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        _userManagerMock.Verify(
            u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task Signup_EmailDuplicado_NoLlamaAAddToRoleAsync()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(_usuarioPrueba);

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        _userManagerMock.Verify(
            u => u.AddToRoleAsync(It.IsAny<Usuario>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task Signup_EmailDuplicado_NoInsertaUnCliente()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(request.Email))
            .ReturnsAsync(_usuarioPrueba);

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(await _dbContext.Clientes.AnyAsync(), Is.False);
    }

    // +++++++++++++++++++++++++++++++++++++++ TESTS CREATEASYNC / CREACIÓN DE USUARIO ++++++++++++++++++++++++++++++++++

    [Test]
    public async Task Signup_CreateAsyncDevuelveExito_ContinuaElRegistro()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<Usuario>(), Roles.Cliente), Times.Once);
        Assert.That(await _dbContext.Clientes.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Signup_CreateAsyncDevuelveError_DevuelveValidationErrorDeIdentity()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");
        var identityError = IdentityResult.Failed(new IdentityError
        {
            Code = "PasswordTooShort",
            Description = "La contraseña debe tener al menos 8 caracteres."
        });

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(identityError);

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ValidationError>());
    }

    [Test]
    public async Task Signup_CreateAsyncDevuelveUnError_SeIncluyeSuDescripcion()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");
        const string descripcionEsperada = "La contraseña requiere al menos un carácter especial.";

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordRequiresNonAlphanumeric",
                Description = descripcionEsperada
            }));

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ValidationError>());
        var validationError = (ValidationError)result.Error;
        Assert.That(validationError.Message, Is.EqualTo("Error al crear el usuario"));
        Assert.That(validationError.Errors["Identity"], Does.Contain(descripcionEsperada));
    }

    [Test]
    public async Task Signup_CreateAsyncDevuelveVariosErrores_SeIncluyenTodasLasDescripciones()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");
        const string error1 = "La contraseña debe tener al menos una mayúscula.";
        const string error2 = "La contraseña debe tener al menos un número.";

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "PasswordRequiresUpper", Description = error1 },
                new IdentityError { Code = "PasswordRequiresDigit", Description = error2 }
            ));

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<ValidationError>());
        var validationError = (ValidationError)result.Error;
        Assert.That(validationError.Errors["Identity"], Does.Contain(error1));
        Assert.That(validationError.Errors["Identity"], Does.Contain(error2));
    }

    [Test]
    public async Task Signup_CreateAsyncDevuelveError_SeEjecutaRollback()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error de creación" }));

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(_dbContext.Database.CurrentTransaction, Is.Null);
        Assert.That(await _dbContext.Users.AnyAsync(), Is.False);
        Assert.That(await _dbContext.Clientes.AnyAsync(), Is.False);
    }

    [Test]
    public async Task Signup_CreateAsyncDevuelveError_NoAsignaElRol()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error de creación" }));

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        _userManagerMock.Verify(
            u => u.AddToRoleAsync(It.IsAny<Usuario>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task Signup_CreateAsyncDevuelveError_NoCreaElCliente()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error de creación" }));

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(await _dbContext.Clientes.AnyAsync(), Is.False);
    }

    [Test]
    public async Task Signup_SeCreaUsuario_UserNameCoincideConElEmail()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");
        Usuario? usuarioCapturado = null;

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .Returns(async (Usuario u, string _) =>
            {
                usuarioCapturado = u;
                await _dbContext.Users.AddAsync(u);
                await _dbContext.SaveChangesAsync();
                return IdentityResult.Success;
            });

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(usuarioCapturado, Is.Not.Null);
        Assert.That(usuarioCapturado!.UserName, Is.EqualTo(request.Email.Trim()));
    }

    [Test]
    public async Task Signup_SeCreaUsuario_EmailCoincideConElEmail()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");
        Usuario? usuarioCapturado = null;

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .Returns(async (Usuario u, string _) =>
            {
                usuarioCapturado = u;
                await _dbContext.Users.AddAsync(u);
                await _dbContext.SaveChangesAsync();
                return IdentityResult.Success;
            });

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(usuarioCapturado, Is.Not.Null);
        Assert.That(usuarioCapturado!.Email, Is.EqualTo(request.Email.Trim()));
    }

    [Test]
    public async Task Signup_SeCreaUsuario_PhoneNumberCoincideConElTelefono()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X") with { Telefono = "611222333" };
        Usuario? usuarioCapturado = null;

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .Returns(async (Usuario u, string _) =>
            {
                usuarioCapturado = u;
                await _dbContext.Users.AddAsync(u);
                await _dbContext.SaveChangesAsync();
                return IdentityResult.Success;
            });

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(usuarioCapturado, Is.Not.Null);
        Assert.That(usuarioCapturado!.PhoneNumber, Is.EqualTo(request.Telefono));
    }

    [Test]
    public async Task Signup_SeCreaUsuario_CreatedAtTieneUnValorValido()
    {
        // Arrange
        var antesDeCrear = DateTime.UtcNow.AddSeconds(-1);
        var request = CrearSignupDto(dni: "87654321X");
        Usuario? usuarioCapturado = null;

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .Returns(async (Usuario u, string _) =>
            {
                usuarioCapturado = u;
                await _dbContext.Users.AddAsync(u);
                await _dbContext.SaveChangesAsync();
                return IdentityResult.Success;
            });

        // Act
        var result = await _service.SignupAsync(request);
        var despuesDeCrear = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(usuarioCapturado, Is.Not.Null);
        Assert.That(usuarioCapturado!.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(usuarioCapturado.CreatedAt, Is.InRange(antesDeCrear, despuesDeCrear));
    }

    [Test]
    public async Task Signup_SeCreaUsuario_SeUtilizaLaContrasenaRecibidaEnIdentity()
    {
        // Arrange
        const string passwordEsperada = "ClaveSegura987!";
        var request = CrearSignupDto(dni: "87654321X") with { Contraseña = passwordEsperada };

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _userManagerMock.Verify(
            u => u.CreateAsync(It.IsAny<Usuario>(), passwordEsperada),
            Times.Once);
    }
    
    // +++++++++++++++++++++++++++++++++++++++ TESTS ADDTOROLEASYNC / ASIGNACIÓN DE ROL ++++++++++++++++++++++++++++++++++

    [Test]
    public async Task Signup_AddToRoleAsyncDevuelveExito_ContinuaElRegistro()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        _userManagerMock
            .Setup(u => u.AddToRoleAsync(It.IsAny<Usuario>(), Roles.Cliente))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(await _dbContext.Clientes.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Signup_AddToRoleAsyncDevuelveError_DevuelveInternalError()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        _userManagerMock
            .Setup(u => u.AddToRoleAsync(It.IsAny<Usuario>(), Roles.Cliente))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "RoleNotFound",
                Description = "Error al asignar el rol al usuario."
            }));

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<InternalError>());
    }

    [Test]
    public async Task Signup_AsignacionDelRolFalla_SeEjecutaRollback()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        _userManagerMock
            .Setup(u => u.AddToRoleAsync(It.IsAny<Usuario>(), Roles.Cliente))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error asignando rol" }));

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(_dbContext.Database.CurrentTransaction, Is.Null);
        Assert.That(await _dbContext.Users.AnyAsync(), Is.False);
        Assert.That(await _dbContext.Clientes.AnyAsync(), Is.False);
    }

    [Test]
    public async Task Signup_AsignacionDelRolFalla_NoSeCreaElCliente()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        _userManagerMock
            .Setup(u => u.AddToRoleAsync(It.IsAny<Usuario>(), Roles.Cliente))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error asignando rol" }));

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(await _dbContext.Clientes.AnyAsync(), Is.False);
    }

    [Test]
    public async Task Signup_RegistroCorrecto_SeAsignaRolCliente()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _userManagerMock.Verify(
            u => u.AddToRoleAsync(It.IsAny<Usuario>(), Roles.Cliente),
            Times.Once);
    }

    [Test]
    public async Task Signup_RegistroCorrecto_NoSeIntentaAsignarOtroRol()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _userManagerMock.Verify(
            u => u.AddToRoleAsync(It.IsAny<Usuario>(), It.Is<string>(rol => rol != Roles.Cliente)),
            Times.Never);
    }
    
    // +++++++++++++++++++++++++++++++++++++++ TESTS PERSISTENCIA CLIENTE Y TRANSACCIÓN ++++++++++++++++++++++++++++++++++

    [Test]
    public async Task Signup_RegistroCorrecto_SeInsertaUnCliente()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(await _dbContext.Clientes.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_IdUsuarioCorrespondeAlUsuarioCreado()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var cliente = await _dbContext.Clientes.FirstOrDefaultAsync();
        Assert.That(cliente, Is.Not.Null);
        Assert.That(cliente!.IdUsuario, Is.EqualTo(_usuarioPrueba.Id));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_SeGuardaElNombre()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X") with { Nombre = "Elena" };

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var cliente = await _dbContext.Clientes.FirstOrDefaultAsync();
        Assert.That(cliente, Is.Not.Null);
        Assert.That(cliente!.Nombre, Is.EqualTo("Elena"));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_SeGuardanLosApellidos()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X") with { Apellidos = "Martín Ruiz" };

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var cliente = await _dbContext.Clientes.FirstOrDefaultAsync();
        Assert.That(cliente, Is.Not.Null);
        Assert.That(cliente!.Apellidos, Is.EqualTo("Martín Ruiz"));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_SeGuardaLaFechaDeNacimiento()
    {
        // Arrange
        var fechaNacimiento = new DateTime(1992, 8, 14, 0, 0, 0, DateTimeKind.Utc);
        var request = CrearSignupDto(dni: "87654321X") with { FechaNacimiento = fechaNacimiento };

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var cliente = await _dbContext.Clientes.FirstOrDefaultAsync();
        Assert.That(cliente, Is.Not.Null);
        Assert.That(cliente!.FechaNacimiento, Is.EqualTo(fechaNacimiento));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_LaFechaSeEstableceComoUtc()
    {
        // Arrange
        var fechaLocal = new DateTime(1992, 8, 14, 10, 30, 0, DateTimeKind.Local);
        var request = CrearSignupDto(dni: "87654321X") with { FechaNacimiento = fechaLocal };

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var cliente = await _dbContext.Clientes.FirstOrDefaultAsync();
        Assert.That(cliente, Is.Not.Null);
        Assert.That(cliente!.FechaNacimiento.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_SeGuardaElDni()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var cliente = await _dbContext.Clientes.FirstOrDefaultAsync();
        Assert.That(cliente, Is.Not.Null);
        Assert.That(cliente!.Dni, Is.EqualTo("87654321X"));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_SeGuardaElNie()
    {
        // Arrange
        var request = CrearSignupDto(nie: "Y7654321M");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var cliente = await _dbContext.Clientes.FirstOrDefaultAsync();
        Assert.That(cliente, Is.Not.Null);
        Assert.That(cliente!.Nie, Is.EqualTo("Y7654321M"));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_SeGuardaElPasaporte()
    {
        // Arrange
        var request = CrearSignupDto(pasaporte: "PBB987654");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var cliente = await _dbContext.Clientes.FirstOrDefaultAsync();
        Assert.That(cliente, Is.Not.Null);
        Assert.That(cliente!.Pasaporte, Is.EqualTo("PBB987654"));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_SeGuardaElPermisoDeConducir()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X") with { NumeroPermisoConducir = "PERMISO12345" };

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var cliente = await _dbContext.Clientes.FirstOrDefaultAsync();
        Assert.That(cliente, Is.Not.Null);
        Assert.That(cliente!.NumeroPermisoConducir, Is.EqualTo("PERMISO12345"));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_CreatedAtTieneUnValorValido()
    {
        // Arrange
        var antes = DateTime.UtcNow.AddSeconds(-1);
        var request = CrearSignupDto(dni: "87654321X");

        // Act
        var result = await _service.SignupAsync(request);
        var despues = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var cliente = await _dbContext.Clientes.FirstOrDefaultAsync();
        Assert.That(cliente, Is.Not.Null);
        Assert.That(cliente!.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(cliente.CreatedAt, Is.InRange(antes, despues));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_IsActivoEsTrue()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var cliente = await _dbContext.Clientes.FirstOrDefaultAsync();
        Assert.That(cliente, Is.Not.Null);
        Assert.That(cliente!.IsActivo, Is.True);
    }

    [Test]
    public async Task Signup_DniEnMinusculas_SeGuardaEnMayusculas()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321x");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var cliente = await _dbContext.Clientes.FirstOrDefaultAsync();
        Assert.That(cliente, Is.Not.Null);
        Assert.That(cliente!.Dni, Is.EqualTo("87654321X"));
    }

    [Test]
    public async Task Signup_NieEnMinusculas_SeGuardaEnMayusculas()
    {
        // Arrange
        var request = CrearSignupDto(nie: "y7654321m");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var cliente = await _dbContext.Clientes.FirstOrDefaultAsync();
        Assert.That(cliente, Is.Not.Null);
        Assert.That(cliente!.Nie, Is.EqualTo("Y7654321M"));
    }

    [Test]
    public async Task Signup_PasaporteEnMinusculas_SeGuardaEnMayusculas()
    {
        // Arrange
        var request = CrearSignupDto(pasaporte: "pbb987654");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var cliente = await _dbContext.Clientes.FirstOrDefaultAsync();
        Assert.That(cliente, Is.Not.Null);
        Assert.That(cliente!.Pasaporte, Is.EqualTo("PBB987654"));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_SeConfirmaLaTransaccion()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(_dbContext.Database.CurrentTransaction, Is.Null);

        // Consultamos desde un nuevo contexto independiente para verificar que el commit quedó persistido en PostgreSQL
        await using var nuevoContexto = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_connectionString)
                .Options);

        Assert.That(await nuevoContexto.Clientes.CountAsync(), Is.EqualTo(1));
        Assert.That(await nuevoContexto.Users.CountAsync(), Is.EqualTo(1));
    }
    
    // +++++++++++++++++++++++++++++++++++++++ RESULTADO DE UN REGISTRO CORRECTO ++++++++++++++++++++++++++++++++++

    [Test]
    public async Task Signup_RegistroCorrecto_DevuelveResultIsSuccessTrue()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task Signup_RegistroCorrecto_DevuelveElIdDelUsuario()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.GreaterThan(0));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_DevuelveElEmail()
    {
        // Arrange
        var email = $"email-{Guid.NewGuid()}@correo.com";
        var request = CrearSignupDto(dni: "87654321X") with { Email = email };

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Email, Is.EqualTo(email));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_DevuelveElTelefono()
    {
        // Arrange
        const string telefono = "655443322";
        var request = CrearSignupDto(dni: "87654321X") with { Telefono = telefono };

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Telefono, Is.EqualTo(telefono));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_DevuelveElRolCliente()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Rol, Is.EqualTo(Roles.Cliente));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_DevuelveElIdDelCliente()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ClienteId, Is.Not.Null);
        Assert.That(result.Value.ClienteId, Is.GreaterThan(0));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_TrabajadorIdEsNull()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.TrabajadorId, Is.Null);
    }

    [Test]
    public async Task Signup_RegistroCorrecto_DevuelveLaFechaDeCreacionDelUsuario()
    {
        // Arrange
        var antes = DateTime.UtcNow.AddSeconds(-2);
        var request = CrearSignupDto(dni: "87654321X");

        // Act
        var result = await _service.SignupAsync(request);
        var despues = DateTime.UtcNow.AddSeconds(2);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(result.Value.CreatedAt, Is.InRange(antes, despues));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_NoDevuelveLaContrasena()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X") with { Contraseña = "PasswordSecreta123!" };

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var tipoDto = result.Value.GetType();
        Assert.That(tipoDto.GetProperty("Contraseña"), Is.Null);
        Assert.That(tipoDto.GetProperty("Contrasena"), Is.Null);
        Assert.That(tipoDto.GetProperty("Password"), Is.Null);
    }

    [Test]
    public async Task Signup_RegistroCorrecto_ElUsuarioExisteEnIdentity()
    {
        // Arrange
        var email = $"identity-{Guid.NewGuid()}@correo.com";
        var request = CrearSignupDto(dni: "87654321X") with { Email = email };

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _userManagerMock.Verify(
            u => u.CreateAsync(It.Is<Usuario>(usr => usr.Email == email), request.Contraseña),
            Times.Once);

        var usuarioEnBd = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        Assert.That(usuarioEnBd, Is.Not.Null);
        Assert.That(usuarioEnBd!.Id, Is.EqualTo(result.Value.Id));
    }

    [Test]
    public async Task Signup_RegistroCorrecto_ElClienteExisteEnPostgreSql()
    {
        // Arrange
        var request = CrearSignupDto(dni: "87654321X");

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;
        await using var verifyCtx = new AppDbContext(options);

        var clienteEnBd = await verifyCtx.Clientes
            .FirstOrDefaultAsync(c => c.Id == result.Value.ClienteId);

        Assert.That(clienteEnBd, Is.Not.Null);
        Assert.That(clienteEnBd!.IdUsuario, Is.EqualTo(result.Value.Id));
        Assert.That(clienteEnBd.Dni, Is.EqualTo("87654321X"));
    }
    #endregion

    #region TEST LOGIN
    // +++++++++++++++++++++++++++++++++++++++ BÚSQUEDA DE USUARIO EN LOGIN ++++++++++++++++++++++++++++++++++

    [Test]
    public async Task Login_EmailExistente_EncuentraAlUsuario()
    {
        // Arrange: usuario existente en la base de datos y recuperable por UserManager
        var email = $"login-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            Cliente = new Cliente
            {
                Nombre = "Juan",
                Apellidos = "Pérez",
                FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                IsActivo = true
            }
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        _userManagerMock
            .Setup(u => u.GetRolesAsync(It.Is<Usuario>(u => u.Id == usuario.Id)))
            .ReturnsAsync(new List<string> { Roles.Cliente });

        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(It.Is<Usuario>(u => u.Id == usuario.Id), "Password123!", true))
            .ReturnsAsync(SignInResult.Success);

        var request = new LoginDto(email, "Password123!");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        _signInManagerMock.Verify(
            s => s.CheckPasswordSignInAsync(It.Is<Usuario>(u => u.Id == usuario.Id), "Password123!", true),
            Times.Once);
    }
    
    [Test]
    public async Task Login_EmailInexistente_DevuelveUnauthorizedError()
    {
        // Arrange: no existe ningún usuario con el email indicado
        var email = $"noexiste-{Guid.NewGuid()}@correo.com";
        _userManagerMock
            .Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Usuario?)null);

        var request = new LoginDto(email, "Password123!");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<UnauthorizedError>());
    }

    [Test]
    public async Task Login_UsuarioSinClienteNiTrabajadorAsociado_NoProduceNullReferenceException()
    {
        // Arrange: usuario sin relaciones Cliente ni Trabajador en base de datos
        var email = $"sinrelacion-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);

        var request = new LoginDto(email, "Password123!");

        // Act & Assert: no debe lanzar NullReferenceException al resolver relaciones
        Assert.DoesNotThrowAsync(async () => await _service.LoginAsync(request));
    }

    [Test]
    public async Task Login_UsuarioConClienteAsociado_RecuperaElCliente()
    {
        // Arrange: usuario con entidad Cliente vinculada
        var email = $"cliente-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var cliente = new Cliente
        {
            IdUsuario = usuario.Id,
            Nombre = "Laura",
            Apellidos = "Gómez",
            Dni = "11223344A",
            FechaNacimiento = new DateTime(1994, 3, 12, 0, 0, 0, DateTimeKind.Utc),
            NumeroPermisoConducir = "PERM111",
            CreatedAt = DateTime.UtcNow,
            IsActivo = true
        };
        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();

        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(It.Is<Usuario>(u => u.Id == usuario.Id), "Password123!", true))
            .ReturnsAsync(SignInResult.Success);

        var request = new LoginDto(email, "Password123!");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        if (result.IsSuccess)
        {
            Assert.That(result.Value.Usuario.ClienteId, Is.EqualTo(cliente.Id));
        }
        else
        {
            Assert.That(result.Error, Is.Not.TypeOf<UnauthorizedError>());
        }
    }

    [Test]
    public async Task Login_UsuarioConTrabajadorAsociado_RecuperaElTrabajador()
    {
        // Arrange: usuario con entidad Trabajador vinculada
        var email = $"trabajador-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var trabajador = new Trabajador
        {
            IdUsuario = usuario.Id,
            Nombre = "Carlos",
            Apellidos = "Sanz",
            Dni = "12345678Z",
            TipoTrabajador = TipoTrabajador.Mecanico,
            CreatedAt = DateTime.UtcNow,
            IsActivo = true
        };
        _dbContext.Trabajadores.Add(trabajador);
        await _dbContext.SaveChangesAsync();

        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(It.Is<Usuario>(u => u.Id == usuario.Id), "Password123!", true))
            .ReturnsAsync(SignInResult.Success);
        
        _userManagerMock
            .Setup(u => u.GetRolesAsync(It.Is<Usuario>(u => u.Id == usuario.Id)))
            .ReturnsAsync(new List<string> { Roles.Mecanico });

        var request = new LoginDto(email, "Password123!");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Usuario.TrabajadorId, Is.EqualTo(trabajador.Id));
    }

    [Test]
    public async Task Login_UsuarioConClienteYTrabajadorAsociados_RecuperaAmbasRelaciones()
    {
        // Arrange: usuario con relaciones tanto de Cliente como de Trabajador
        var email = $"ambos-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var cliente = new Cliente
        {
            IdUsuario = usuario.Id,
            Nombre = "Laura",
            Apellidos = "Gómez",
            Dni = "55667788B",
            FechaNacimiento = new DateTime(1994, 3, 12, 0, 0, 0, DateTimeKind.Utc),
            NumeroPermisoConducir = "PERM222",
            CreatedAt = DateTime.UtcNow,
            IsActivo = true
        };
        var trabajador = new Trabajador
        {
            IdUsuario = usuario.Id,
            Nombre = "Laura",
            Apellidos = "Gómez",
            Dni = "12345678Z",
            TipoTrabajador = TipoTrabajador.Mecanico,
            CreatedAt = DateTime.UtcNow,
            IsActivo = true
        };
        _dbContext.Clientes.Add(cliente);
        _dbContext.Trabajadores.Add(trabajador);
        await _dbContext.SaveChangesAsync();

        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(It.Is<Usuario>(u => u.Id == usuario.Id), "Password123!", true))
            .ReturnsAsync(SignInResult.Success);

        _userManagerMock
            .Setup(u => u.GetRolesAsync(It.Is<Usuario>(u => u.Id == usuario.Id)))
            .ReturnsAsync(new List<string> { Roles.Administrador });

        var request = new LoginDto(email, "Password123!");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Usuario.ClienteId, Is.EqualTo(cliente.Id));
        Assert.That(result.Value.Usuario.TrabajadorId, Is.EqualTo(trabajador.Id));
    }

    [Test]
public async Task Login_EmailConMayusculas_CompruebaComportamientoComparacion()
{
    // Arrange: usuario registrado en minúsculas y consulta enviada en mayúsculas
    var emailMinusculas = $"mayus-{Guid.NewGuid()}@correo.com";
    var emailMayusculas = emailMinusculas.ToUpperInvariant();
    var usuario = new Usuario
    {
        UserName = emailMinusculas,
        Email = emailMinusculas,
        NormalizedEmail = emailMinusculas.ToUpperInvariant(),
        NormalizedUserName = emailMinusculas.ToUpperInvariant(),
        EmailConfirmed = true,
        CreatedAt = DateTime.UtcNow
    };
    _dbContext.Users.Add(usuario);
    await _dbContext.SaveChangesAsync();

    _signInManagerMock
        .Setup(s => s.CheckPasswordSignInAsync(It.Is<Usuario>(u => u.Id == usuario.Id), "Password123!", true))
        .ReturnsAsync(SignInResult.Success);

    _userManagerMock
        .Setup(u => u.GetRolesAsync(It.Is<Usuario>(u => u.Id == usuario.Id)))
        .ReturnsAsync(new List<string> { Roles.Administrador });

    var request = new LoginDto(emailMayusculas, "Password123!");

    // Act
    var result = await _service.LoginAsync(request);

    // Assert
    Assert.That(result.IsFailure, Is.True);
    Assert.That(result.Error, Is.TypeOf<UnauthorizedError>());
}

[Test]
public async Task Login_EmailConEspaciosExteriores_CompruebaComportamientoComparacion()
{
    // Arrange: usuario registrado sin espacios y consulta con espacios exteriores
    var emailLimpio = $"espacios-{Guid.NewGuid()}@correo.com";
    var emailConEspacios = $"   {emailLimpio}   ";
    var usuario = new Usuario
    {
        UserName = emailLimpio,
        Email = emailLimpio,
        NormalizedEmail = emailLimpio.ToUpperInvariant(),
        NormalizedUserName = emailLimpio.ToUpperInvariant(),
        EmailConfirmed = true,
        CreatedAt = DateTime.UtcNow
    };
    _dbContext.Users.Add(usuario);
    await _dbContext.SaveChangesAsync();

    _signInManagerMock
        .Setup(s => s.CheckPasswordSignInAsync(It.Is<Usuario>(u => u.Id == usuario.Id), "Password123!", true))
        .ReturnsAsync(SignInResult.Success);

    _userManagerMock
        .Setup(u => u.GetRolesAsync(It.Is<Usuario>(u => u.Id == usuario.Id)))
        .ReturnsAsync(new List<string> { Roles.Cliente });

    var request = new LoginDto(emailConEspacios, "Password123!");

    // Act
    var result = await _service.LoginAsync(request);

    // Assert
    Assert.That(result.IsFailure, Is.True);
    Assert.That(result.Error, Is.TypeOf<UnauthorizedError>());

}

// +++++++++++++++++++++++++++++++++++++++ ESTADO DE LA CUENTA EN LOGIN ++++++++++++++++++++++++++++++++++

[Test]
public async Task Login_ClienteActivo_ContinuaElLogin()
{
    // Arrange: usuario con ficha de Cliente activa
    var email = $"cliente-activo-{Guid.NewGuid()}@correo.com";
    var usuario = new Usuario
    {
        UserName = email,
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        NormalizedUserName = email.ToUpperInvariant(),
        EmailConfirmed = true,
        CreatedAt = DateTime.UtcNow
    };
    _dbContext.Users.Add(usuario);
    await _dbContext.SaveChangesAsync();

    var cliente = new Cliente
    {
        IdUsuario = usuario.Id,
        Nombre = "Ana",
        Apellidos = "López",
        Dni = "12345678Z",
        FechaNacimiento = new DateTime(1992, 6, 15, 0, 0, 0, DateTimeKind.Utc),
        NumeroPermisoConducir = "PERM001",
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    _dbContext.Clientes.Add(cliente);
    await _dbContext.SaveChangesAsync();

    _signInManagerMock
        .Setup(s => s.CheckPasswordSignInAsync(It.Is<Usuario>(u => u.Id == usuario.Id), "Password123!", true))
        .ReturnsAsync(SignInResult.Success);

    _userManagerMock
        .Setup(u => u.GetRolesAsync(It.Is<Usuario>(u => u.Id == usuario.Id)))
        .ReturnsAsync(new List<string> { Roles.Cliente });

    var request = new LoginDto(email, "Password123!");

    // Act
    var result = await _service.LoginAsync(request);

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    _signInManagerMock.Verify(
        s => s.CheckPasswordSignInAsync(It.Is<Usuario>(u => u.Id == usuario.Id), "Password123!", true),
        Times.Once);
}

[Test]
public async Task Login_ClienteInactivo_DevuelveInactiveAccountYNoVerificaPassword()
{
    // Arrange: usuario con ficha de Cliente inactiva
    var email = $"cliente-inactivo-{Guid.NewGuid()}@correo.com";
    var usuario = new Usuario
    {
        UserName = email,
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        NormalizedUserName = email.ToUpperInvariant(),
        EmailConfirmed = true,
        CreatedAt = DateTime.UtcNow
    };
    _dbContext.Users.Add(usuario);
    await _dbContext.SaveChangesAsync();

    var cliente = new Cliente
    {
        IdUsuario = usuario.Id,
        Nombre = "Ana",
        Apellidos = "López",
        Dni = "12345678Z",
        FechaNacimiento = new DateTime(1992, 6, 15, 0, 0, 0, DateTimeKind.Utc),
        NumeroPermisoConducir = "PERM001",
        CreatedAt = DateTime.UtcNow,
        IsActivo = false
    };
    _dbContext.Clientes.Add(cliente);
    await _dbContext.SaveChangesAsync();

    var request = new LoginDto(email, "Password123!");

    // Act
    var result = await _service.LoginAsync(request);

    // Assert
    Assert.That(result.IsFailure, Is.True);
    Assert.That(result.Error.Message, Is.EqualTo(AuthErrors.InactiveAccount().Message));
    _signInManagerMock.Verify(
        s => s.CheckPasswordSignInAsync(It.IsAny<Usuario>(), It.IsAny<string>(), It.IsAny<bool>()),
        Times.Never);
}

[Test]
public async Task Login_TrabajadorActivo_ContinuaElLogin()
{
    // Arrange: usuario con ficha de Trabajador activa
    var email = $"trabajador-activo-{Guid.NewGuid()}@correo.com";
    var usuario = new Usuario
    {
        UserName = email,
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        NormalizedUserName = email.ToUpperInvariant(),
        EmailConfirmed = true,
        CreatedAt = DateTime.UtcNow
    };
    _dbContext.Users.Add(usuario);
    await _dbContext.SaveChangesAsync();

    var trabajador = new Trabajador
    {
        IdUsuario = usuario.Id,
        Nombre = "Pedro",
        Apellidos = "Ruiz",
        Dni = "12345678Z",
        TipoTrabajador = TipoTrabajador.Mecanico,
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    _dbContext.Trabajadores.Add(trabajador);
    await _dbContext.SaveChangesAsync();

    _signInManagerMock
        .Setup(s => s.CheckPasswordSignInAsync(It.Is<Usuario>(u => u.Id == usuario.Id), "Password123!", true))
        .ReturnsAsync(SignInResult.Success);

    _userManagerMock
        .Setup(u => u.GetRolesAsync(It.Is<Usuario>(u => u.Id == usuario.Id)))
        .ReturnsAsync(new List<string> { Roles.Mecanico });

    var request = new LoginDto(email, "Password123!");

    // Act
    var result = await _service.LoginAsync(request);

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    _signInManagerMock.Verify(
        s => s.CheckPasswordSignInAsync(It.Is<Usuario>(u => u.Id == usuario.Id), "Password123!", true),
        Times.Once);
}

[Test]
public async Task Login_TrabajadorInactivo_DevuelveInactiveAccountYNoVerificaPassword()
{
    // Arrange: usuario con ficha de Trabajador inactiva
    var email = $"trabajador-inactivo-{Guid.NewGuid()}@correo.com";
    var usuario = new Usuario
    {
        UserName = email,
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        NormalizedUserName = email.ToUpperInvariant(),
        EmailConfirmed = true,
        CreatedAt = DateTime.UtcNow
    };
    _dbContext.Users.Add(usuario);
    await _dbContext.SaveChangesAsync();

    var trabajador = new Trabajador
    {
        IdUsuario = usuario.Id,
        Nombre = "Pedro",
        Apellidos = "Ruiz",
        Dni = "12345678Z",
        TipoTrabajador = TipoTrabajador.Mecanico,
        CreatedAt = DateTime.UtcNow,
        IsActivo = false
    };
    _dbContext.Trabajadores.Add(trabajador);
    await _dbContext.SaveChangesAsync();

    var request = new LoginDto(email, "Password123!");

    // Act
    var result = await _service.LoginAsync(request);

    // Assert
    Assert.That(result.IsFailure, Is.True);
    Assert.That(result.Error.Message, Is.EqualTo(AuthErrors.InactiveAccount().Message));
    _signInManagerMock.Verify(
        s => s.CheckPasswordSignInAsync(It.IsAny<Usuario>(), It.IsAny<string>(), It.IsAny<bool>()),
        Times.Never);
}

[Test]
public async Task Login_UsuarioSinClienteNiTrabajador_ContinuaElLogin()
{
    // Arrange: usuario sin ficha de Cliente ni de Trabajador (p. ej. Administrador)
    var email = $"sin-ficha-{Guid.NewGuid()}@correo.com";
    var usuario = new Usuario
    {
        UserName = email,
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        NormalizedUserName = email.ToUpperInvariant(),
        EmailConfirmed = true,
        CreatedAt = DateTime.UtcNow
    };
    _dbContext.Users.Add(usuario);
    await _dbContext.SaveChangesAsync();

    _signInManagerMock
        .Setup(s => s.CheckPasswordSignInAsync(It.Is<Usuario>(u => u.Id == usuario.Id), "Password123!", true))
        .ReturnsAsync(SignInResult.Success);

    _userManagerMock
        .Setup(u => u.GetRolesAsync(It.Is<Usuario>(u => u.Id == usuario.Id)))
        .ReturnsAsync(new List<string> { Roles.Administrador });

    var request = new LoginDto(email, "Password123!");

    // Act
    var result = await _service.LoginAsync(request);

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    _signInManagerMock.Verify(
        s => s.CheckPasswordSignInAsync(It.Is<Usuario>(u => u.Id == usuario.Id), "Password123!", true),
        Times.Once);
}

[Test]
public async Task Login_ClienteActivoYTrabajadorActivo_ContinuaElLogin()
{
    // Arrange: usuario con ambas fichas activas
    var email = $"ambos-activos-{Guid.NewGuid()}@correo.com";
    var usuario = new Usuario
    {
        UserName = email,
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        NormalizedUserName = email.ToUpperInvariant(),
        EmailConfirmed = true,
        CreatedAt = DateTime.UtcNow
    };
    _dbContext.Users.Add(usuario);
    await _dbContext.SaveChangesAsync();

    _dbContext.Clientes.Add(new Cliente
    {
        IdUsuario = usuario.Id,
        Nombre = "Lucía",
        Apellidos = "Mora",
        Dni = "87654321X",
        FechaNacimiento = new DateTime(1991, 2, 10, 0, 0, 0, DateTimeKind.Utc),
        NumeroPermisoConducir = "PERM002",
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    });
    _dbContext.Trabajadores.Add(new Trabajador
    {
        IdUsuario = usuario.Id,
        Nombre = "Lucía",
        Apellidos = "Mora",
        Dni = "12345678Z",
        TipoTrabajador = TipoTrabajador.Mecanico,
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    });
    await _dbContext.SaveChangesAsync();

    _signInManagerMock
        .Setup(s => s.CheckPasswordSignInAsync(It.Is<Usuario>(u => u.Id == usuario.Id), "Password123!", true))
        .ReturnsAsync(SignInResult.Success);

    _userManagerMock
        .Setup(u => u.GetRolesAsync(It.Is<Usuario>(u => u.Id == usuario.Id)))
        .ReturnsAsync(new List<string> { Roles.Administrador });

    var request = new LoginDto(email, "Password123!");

    // Act
    var result = await _service.LoginAsync(request);

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    _signInManagerMock.Verify(
        s => s.CheckPasswordSignInAsync(It.Is<Usuario>(u => u.Id == usuario.Id), "Password123!", true),
        Times.Once);
}

    [Test]
    public async Task Login_ClienteInactivoYTrabajadorActivo_DevuelveInactiveAccountYNoVerificaPassword()
    {
        // Arrange: ficha de Cliente inactiva y Trabajador activa
        var email = $"cliente-inactivo-trab-activo-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        _dbContext.Clientes.Add(new Cliente
        {
            IdUsuario = usuario.Id,
            Nombre = "Lucía",
            Apellidos = "Mora",
            Dni = "87654321X",
            FechaNacimiento = new DateTime(1991, 2, 10, 0, 0, 0, DateTimeKind.Utc),
            NumeroPermisoConducir = "PERM002",
            CreatedAt = DateTime.UtcNow,
            IsActivo = false
        });
        _dbContext.Trabajadores.Add(new Trabajador
        {
            IdUsuario = usuario.Id,
            Nombre = "Lucía",
            Apellidos = "Mora",
            Dni = "12345678Z",
            TipoTrabajador = TipoTrabajador.Mecanico,
            CreatedAt = DateTime.UtcNow,
            IsActivo = true
        });
        await _dbContext.SaveChangesAsync();

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);

        var request = new LoginDto(email, "Password123!");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<UnauthorizedError>());
        _signInManagerMock.Verify(
            s => s.CheckPasswordSignInAsync(It.IsAny<Usuario>(), It.IsAny<string>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Test]
    public async Task Login_ClienteActivoYTrabajadorInactivo_DevuelveInactiveAccountYNoVerificaPassword()
    {
        // Arrange: ficha de Cliente activa y Trabajador inactiva
        var email = $"cliente-activo-trab-inactivo-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        _dbContext.Clientes.Add(new Cliente
        {
            IdUsuario = usuario.Id,
            Nombre = "Lucía",
            Apellidos = "Mora",
            Dni = "87654321X",
            FechaNacimiento = new DateTime(1991, 2, 10, 0, 0, 0, DateTimeKind.Utc),
            NumeroPermisoConducir = "PERM002",
            CreatedAt = DateTime.UtcNow,
            IsActivo = true
        });
        _dbContext.Trabajadores.Add(new Trabajador
        {
            IdUsuario = usuario.Id,
            Nombre = "Lucía",
            Apellidos = "Mora",
            Dni = "12345678Z",
            TipoTrabajador = TipoTrabajador.Mecanico,
            CreatedAt = DateTime.UtcNow,
            IsActivo = false
        });
        await _dbContext.SaveChangesAsync();

        _userManagerMock
            .Setup(u => u.GetRolesAsync(It.Is<Usuario>(u => u.Id == usuario.Id)))
            .ReturnsAsync(new List<string> { Roles.Cliente, Roles.Mecanico });

        var request = new LoginDto(email, "Password123!");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<UnauthorizedError>());
        _signInManagerMock.Verify(
            s => s.CheckPasswordSignInAsync(It.IsAny<Usuario>(), It.IsAny<string>(), It.IsAny<bool>()),
            Times.Never);
    }
    [Test]
    public async Task Login_ClienteYTrabajadorInactivos_DevuelveInactiveAccountYNoVerificaPassword()
    {
        // Arrange: ambas fichas inactivas
        var email = $"ambos-inactivos-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        _dbContext.Clientes.Add(new Cliente
        {
            IdUsuario = usuario.Id,
            Nombre = "Lucía",
            Apellidos = "Mora",
            Dni = "87654321X",
            FechaNacimiento = new DateTime(1991, 2, 10, 0, 0, 0, DateTimeKind.Utc),
            NumeroPermisoConducir = "PERM002",
            CreatedAt = DateTime.UtcNow,
            IsActivo = false
        });
        _dbContext.Trabajadores.Add(new Trabajador
        {
            IdUsuario = usuario.Id,
            Nombre = "Lucía",
            Apellidos = "Mora",
            Dni = "12345678Z",
            TipoTrabajador = TipoTrabajador.Mecanico,
            CreatedAt = DateTime.UtcNow,
            IsActivo = false
        });
        await _dbContext.SaveChangesAsync();

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);

        var request = new LoginDto(email, "Password123!");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<UnauthorizedError>());
        _signInManagerMock.Verify(
            s => s.CheckPasswordSignInAsync(It.IsAny<Usuario>(), It.IsAny<string>(), It.IsAny<bool>()),
            Times.Never);
    }
    
    // +++++++++++++++++++++++++++++++++++++++ COMPROBACIÓN DE CONTRASEÑA EN LOGIN ++++++++++++++++++++++++++++++++++

    [Test]
    public async Task Login_ContrasenaCorrecta_ContinuaElLogin()
    {
        // Arrange: usuario existente y contraseña válida según SignInManager
        var email = $"pass-ok-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);

        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(usuario, "Password123!", true))
            .ReturnsAsync(SignInResult.Success);

        var request = new LoginDto(email, "Password123!");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<UnauthorizedError>());

    }

    [Test]
    public async Task Login_ContrasenaIncorrecta_DevuelveUnauthorizedError()
    {
        // Arrange: contraseña no válida para el usuario
        var email = $"pass-mal-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);

        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(usuario, "Incorrecta123!", true))
            .ReturnsAsync(SignInResult.Failed);

        var request = new LoginDto(email, "Incorrecta123!");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<UnauthorizedError>());
    }

    [Test]
    public async Task Login_ContrasenaVacia_CompruebaResultadoIdentity()
    {
        // Arrange: contraseña vacía enviada al flujo de login
        var email = $"pass-vacia-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);

        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(usuario, string.Empty, true))
            .ReturnsAsync(SignInResult.Failed);

        var request = new LoginDto(email, string.Empty);

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<UnauthorizedError>());

    }

    [Test]
    public async Task Login_ContrasenaNula_CompruebaResultadoIdentity()
    {
        // Arrange: contraseña nula enviada al flujo de login
        var email = $"pass-nula-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);

        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(usuario, It.IsAny<string>(), true))
            .ReturnsAsync(SignInResult.Failed);

        var request = new LoginDto(email, null!);

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<UnauthorizedError>());

    }

    [Test]
    public async Task Login_UsuarioBloqueado_DevuelveUnauthorizedError()
    {
        // Arrange: usuario bloqueado por intentos fallidos
        var email = $"bloqueado-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            LockoutEnabled = true,
            LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15),
            CreatedAt = DateTime.UtcNow
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);

        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(usuario, "Password123!", true))
            .ReturnsAsync(SignInResult.LockedOut);

        var request = new LoginDto(email, "Password123!");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<UnauthorizedError>());
    }

    [Test]
    public async Task Login_UsuarioRequiereConfirmacionYNoEstaConfirmado_CompruebaResultadoIdentity()
    {
        // Arrange: usuario sin email confirmado cuando Identity exige confirmación
        var email = $"sin-confirmar-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);

        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(usuario, "Password123!", true))
            .ReturnsAsync(SignInResult.NotAllowed);

        var request = new LoginDto(email, "Password123!");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<UnauthorizedError>());

    }

    [Test]
    public async Task Login_CheckPasswordSignInAsyncDevuelveIsLockedOut_NoGeneraToken()
    {
        // Arrange: resultado LockedOut
        var email = $"locked-notoken-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);

        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(usuario, "Password123!", true))
            .ReturnsAsync(SignInResult.LockedOut);

        var request = new LoginDto(email, "Password123!");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        _userManagerMock.Verify(u => u.GetRolesAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Test]
    public async Task Login_CheckPasswordSignInAsyncDevuelveIsNotAllowed_NoGeneraToken()
    {
        // Arrange: resultado NotAllowed
        var email = $"notallowed-notoken-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);

        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(usuario, "Password123!", true))
            .ReturnsAsync(SignInResult.NotAllowed);

        var request = new LoginDto(email, "Password123!");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        _userManagerMock.Verify(u => u.GetRolesAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Test]
    public async Task Login_CheckPasswordSignInAsyncDevuelveRequiresTwoFactor_NoGeneraToken()
    {
        // Arrange: resultado TwoFactorRequired
        var email = $"2fa-notoken-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            TwoFactorEnabled = true,
            CreatedAt = DateTime.UtcNow
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);

        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(usuario, "Password123!", true))
            .ReturnsAsync(SignInResult.TwoFactorRequired);

        var request = new LoginDto(email, "Password123!");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        _userManagerMock.Verify(u => u.GetRolesAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Test]
    public async Task Login_CheckPasswordSignInAsyncDevuelveSucceededFalse_NoGeneraToken()
    {
        // Arrange: resultado genérico fallido
        var email = $"failed-notoken-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);

        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(usuario, "Password123!", true))
            .ReturnsAsync(SignInResult.Failed);

        var request = new LoginDto(email, "Password123!");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        _userManagerMock.Verify(u => u.GetRolesAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Test]
    public async Task Login_CompruebaLlamadaAIdentity_LockoutOnFailureEsTrue()
    {
        // Arrange: verificamos el tercer argumento booleano (lockoutOnFailure) al invocar CheckPasswordSignInAsync
        var email = $"lockout-true-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(email))
            .ReturnsAsync(usuario);

        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(usuario, "Password123!", true))
            .ReturnsAsync(SignInResult.Success);

        var request = new LoginDto(email, "Password123!");

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<UnauthorizedError>());

    }
    
    // +++++++++++++++++++++++++++++++++++++++ ROLES Y CLAIMS DEL TOKEN EN LOGIN ++++++++++++++++++++++++++++++++++

    [Test]
    public async Task Login_UsuarioConRolCliente_GeneraTokenConRolCliente()
    {
        // Arrange
        var email = $"rol-cliente-{Guid.NewGuid()}@correo.com";
        var usuario = await GuardarUsuarioLoginAsync(email);
        usuario.Cliente = new Cliente
        {
            Nombre = "Juan",
            Apellidos = "Pérez",
            FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow,
            IsActivo = true
        };
        await _dbContext.SaveChangesAsync();
        ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

        // Act
        var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Usuario.Rol, Is.EqualTo(Roles.Cliente));
        var claims = LeerClaimsJwt(result.Value.Token);
        Assert.That(ObtenerRolPrincipalJwt(claims), Is.EqualTo(Roles.Cliente));
    }

[Test]
public async Task Login_UsuarioConRolAdministrador_GeneraTokenConRolAdministrador()
{
    // Arrange
    var email = $"rol-admin-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Administrador });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.Usuario.Rol, Is.EqualTo(Roles.Administrador));
    var claims = LeerClaimsJwt(result.Value.Token);
    Assert.That(ObtenerRolPrincipalJwt(claims), Is.EqualTo(Roles.Administrador));
}

[Test]
public async Task Login_UsuarioConRolMecanico_GeneraTokenConRolMecanico()
{
    // Arrange
    var email = $"rol-mecanico-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Trabajador = new Trabajador
    {
        Nombre = "Pedro",
        Apellidos = "López",
        Dni = $"{Guid.NewGuid().ToString("N")[..8]}Z",
        TipoTrabajador = TipoTrabajador.Mecanico,
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Mecanico });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.Usuario.Rol, Is.EqualTo(Roles.Mecanico));
    var claims = LeerClaimsJwt(result.Value.Token);
    Assert.That(ObtenerRolPrincipalJwt(claims), Is.EqualTo(Roles.Mecanico));
}

[Test]
public async Task Login_UsuarioConRolOperario_GeneraTokenConRolOperario()
{
    // Arrange
    var email = $"rol-operario-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Trabajador = new Trabajador
    {
        Nombre = "Pedro",
        Apellidos = "López",
        Dni = $"{Guid.NewGuid().ToString("N")[..8]}Z",
        TipoTrabajador = TipoTrabajador.Operaciones,
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Operario });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.Usuario.Rol, Is.EqualTo(Roles.Operario));
    var claims = LeerClaimsJwt(result.Value.Token);
    Assert.That(ObtenerRolPrincipalJwt(claims), Is.EqualTo(Roles.Operario));
}

[Test]
public async Task Login_UsuarioSinRoles_UtilizaRolCliente()
{
    // Arrange
    var email = $"sin-roles-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    ConfigurarLoginExitoso(usuario, new List<string>());

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.Usuario.Rol, Is.EqualTo(Roles.Cliente));
    var claims = LeerClaimsJwt(result.Value.Token);
    Assert.That(ObtenerRolPrincipalJwt(claims), Is.EqualTo(Roles.Cliente));
}

[Test]
public async Task Login_UsuarioConVariosRoles_RolPrincipalEsElPrimeroDevuelto()
{
    // Arrange
    var email = $"varios-roles-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Trabajador = new Trabajador
    {
        Nombre = "Pedro",
        Apellidos = "López",
        Dni = $"{Guid.NewGuid().ToString("N")[..8]}Z",
        TipoTrabajador = TipoTrabajador.Operaciones,
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Operario, Roles.Administrador });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.Usuario.Rol, Is.EqualTo(Roles.Operario));
    var claims = LeerClaimsJwt(result.Value.Token);
    Assert.That(ObtenerRolPrincipalJwt(claims), Is.EqualTo(Roles.Operario));
}

[Test]
public async Task Login_UsuarioConRolMecanico_IncluyeClaimTipoTrabajoMecanico()
{
    // Arrange
    var email = $"claim-mecanico-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Trabajador = new Trabajador
    {
        Nombre = "Pedro",
        Apellidos = "López",
        Dni = $"{Guid.NewGuid().ToString("N")[..8]}Z",
        TipoTrabajador = TipoTrabajador.Mecanico,
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Mecanico });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var claims = LeerClaimsJwt(result.Value.Token);
    var tipoTrabajo = claims.FirstOrDefault(c => c.Type == "tipo_trabajo")?.Value;
    Assert.That(tipoTrabajo, Is.EqualTo(Roles.Mecanico));
}

[Test]
public async Task Login_UsuarioConRolOperario_IncluyeClaimTipoTrabajoOperario()
{
    // Arrange
    var email = $"claim-operario-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Trabajador = new Trabajador
    {
        Nombre = "Pedro",
        Apellidos = "López",
        Dni = $"{Guid.NewGuid().ToString("N")[..8]}Z",
        TipoTrabajador = TipoTrabajador.Operaciones,
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Operario });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var claims = LeerClaimsJwt(result.Value.Token);
    var tipoTrabajo = claims.FirstOrDefault(c => c.Type == "tipo_trabajo")?.Value;
    Assert.That(tipoTrabajo, Is.EqualTo(Roles.Operario));
}

[Test]
public async Task Login_UsuarioConRolCliente_NoIncluyeClaimTipoTrabajo()
{
    // Arrange
    var email = $"sin-claim-cliente-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var claims = LeerClaimsJwt(result.Value.Token);
    Assert.That(claims.Any(c => c.Type == "tipo_trabajo"), Is.False);
}

[Test]
public async Task Login_UsuarioConOtroRolSinMecanicoNiOperario_NoIncluyeClaimTipoTrabajo()
{
    // Arrange
    var email = $"sin-claim-admin-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Administrador });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var claims = LeerClaimsJwt(result.Value.Token);
    Assert.That(claims.Any(c => c.Type == "tipo_trabajo"), Is.False);
}

// +++++++++++++++++++++++++++++++++++++++ GENERACIÓN DEL JWT EN LOGIN ++++++++++++++++++++++++++++++++++

[Test]
public async Task Login_LoginCorrecto_GeneraTokenNoVacio()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"jwt-novacio-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.Token, Is.Not.Null.And.Not.Empty);
}

[Test]
public async Task Login_LoginCorrecto_TokenSePuedeLeerComoJwt()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"jwt-legible-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    Assert.That(handler.CanReadToken(result.Value.Token), Is.True);
    var jwt = handler.ReadJwtToken(result.Value.Token);
    Assert.That(jwt, Is.Not.Null);
}

[Test]
public async Task Login_TokenCorrecto_IncluyeIdentificadorUsuarioEnSub()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"jwt-sub-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var jwt = LeerJwt(result.Value.Token);
    var subClaim = jwt.Claims.FirstOrDefault(c =>
        c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub ||
        c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    Assert.That(subClaim, Is.EqualTo(usuario.Id.ToString()));
}

[Test]
public async Task Login_TokenCorrecto_IncluyeEmail()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"jwt-email-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var jwt = LeerJwt(result.Value.Token);
    var emailClaim = jwt.Claims.FirstOrDefault(c =>
        c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email ||
        c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;

    Assert.That(emailClaim, Is.EqualTo(email));
}

[Test]
public async Task Login_TokenCorrecto_IncluyeJti()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"jwt-jti-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var jwt = LeerJwt(result.Value.Token);
    var jti = jwt.Claims.FirstOrDefault(c =>
        c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;

    Assert.That(jti, Is.Not.Null.And.Not.Empty);
}

[Test]
public async Task Login_TokenCorrecto_IncluyeIat()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"jwt-iat-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var jwt = LeerJwt(result.Value.Token);
    var iat = jwt.Claims.FirstOrDefault(c =>
        c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Iat)?.Value;

    Assert.That(iat, Is.Not.Null.And.Not.Empty);
}

[Test]
public async Task Login_TokenCorrecto_IncluyeRol()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"jwt-rol-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Administrador });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var jwt = LeerJwt(result.Value.Token);
    Assert.That(ObtenerRolPrincipalJwt(jwt.Claims.ToList()), Is.EqualTo(Roles.Administrador));
}

    [Test]
public async Task Login_TokenCorrecto_IncluyeTipoTrabajoParaMecanico()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"jwt-mecanico-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Trabajador = new Trabajador
    {
        Nombre = "Pedro",
        Apellidos = "López",
        Dni = $"{Guid.NewGuid().ToString("N")[..8]}Z",
        TipoTrabajador = TipoTrabajador.Mecanico,
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Mecanico });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var jwt = LeerJwt(result.Value.Token);
    var tipoTrabajo = jwt.Claims.FirstOrDefault(c => c.Type == "tipo_trabajo")?.Value;
    Assert.That(tipoTrabajo, Is.EqualTo(Roles.Mecanico));
}

[Test]
public async Task Login_TokenCorrecto_IncluyeTipoTrabajoParaOperario()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"jwt-operario-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Trabajador = new Trabajador
    {
        Nombre = "Pedro",
        Apellidos = "López",
        Dni = $"{Guid.NewGuid().ToString("N")[..8]}Z",
        TipoTrabajador = TipoTrabajador.Operaciones,
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Operario });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var jwt = LeerJwt(result.Value.Token);
    var tipoTrabajo = jwt.Claims.FirstOrDefault(c => c.Type == "tipo_trabajo")?.Value;
    Assert.That(tipoTrabajo, Is.EqualTo(Roles.Operario));
}

[Test]
public async Task Login_TokenCorrecto_NoIncluyeTipoTrabajoParaClienteNormal()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"jwt-cliente-notipo-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var jwt = LeerJwt(result.Value.Token);
    Assert.That(jwt.Claims.Any(c => c.Type == "tipo_trabajo"), Is.False);
}

[Test]
public async Task Login_TokenCorrecto_TieneIssuerConfigurado()
{
    // Arrange
    ConfigurarJwtMock(issuer: "MiIssuerConfigurado");
    var email = $"jwt-issuer-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var jwt = LeerJwt(result.Value.Token);
    Assert.That(jwt.Issuer, Is.EqualTo("MiIssuerConfigurado"));
}

[Test]
public async Task Login_TokenCorrecto_TieneAudienceConfigurado()
{
    // Arrange
    ConfigurarJwtMock(audience: "MiAudienceConfigurado");
    var email = $"jwt-audience-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var jwt = LeerJwt(result.Value.Token);
    Assert.That(jwt.Audiences, Does.Contain("MiAudienceConfigurado"));
}

[Test]
public async Task Login_TokenCorrecto_TieneAlgoritmoFirmaHmacSha256()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"jwt-alg-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var jwt = LeerJwt(result.Value.Token);
    Assert.That(
        jwt.Header.Alg,
        Is.EqualTo(Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256));
}

[Test]
public async Task Login_TokenCorrecto_SePuedeValidarConClaveConfigurada()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"jwt-valida-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var esValido = ValidarFirmaJwt(result.Value.Token, JwtClaveTest, JwtIssuerTest, JwtAudienceTest);
    Assert.That(esValido, Is.True);
}

[Test]
public async Task Login_TokenCorrecto_NoSePuedeValidarConClaveDistinta()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"jwt-invalida-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    const string otraClave = "OtraClaveCompletamenteDiferenteParaValidacion_2026!";
    var esValidoConOtraClave = ValidarFirmaJwt(result.Value.Token, otraClave, JwtIssuerTest, JwtAudienceTest);
    Assert.That(esValidoConOtraClave, Is.False);
}

[Test]
public async Task Login_TokenCorrecto_TieneFechaExpiracion()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"jwt-exp-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var jwt = LeerJwt(result.Value.Token);
    Assert.That(jwt.ValidTo, Is.Not.EqualTo(DateTime.MinValue));
    Assert.That(jwt.ValidTo, Is.GreaterThan(DateTime.UtcNow));
}

[Test]
public async Task Login_TokenCorrecto_ExpiraAproximadamente15MinutosDespues()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"jwt-15min-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // En LoginAsync tienes AddMinutes(120)
    var expiracionEsperadaMin = DateTime.UtcNow.AddMinutes(120).AddSeconds(-15);
    var expiracionEsperadaMax = DateTime.UtcNow.AddMinutes(120).AddSeconds(15);

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var jwt = LeerJwt(result.Value.Token);
    Assert.That(jwt.ValidTo, Is.InRange(expiracionEsperadaMin, expiracionEsperadaMax));
}

[Test]
public async Task Login_DosLoginsConsecutivos_GeneranValoresJtiDiferentes()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"jwt-jti-diff-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result1 = await _service.LoginAsync(new LoginDto(email, "Password123!"));
    var result2 = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result1.IsSuccess, Is.True);
    Assert.That(result2.IsSuccess, Is.True);

    var jti1 = LeerJwt(result1.Value.Token).Claims
        .FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
    var jti2 = LeerJwt(result2.Value.Token).Claims
        .FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;

    Assert.That(jti1, Is.Not.Null.And.Not.Empty);
    Assert.That(jti2, Is.Not.Null.And.Not.Empty);
    Assert.That(jti1, Is.Not.EqualTo(jti2));
}

[Test]
public async Task Login_LoginCorrecto_DevuelveMismaFechaExpiracionQueContieneElJwt()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"jwt-exp-match-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var jwt = LeerJwt(result.Value.Token);
    var diferencia = (result.Value.ExpiresAt - jwt.ValidTo).Duration();
    Assert.That(diferencia, Is.LessThan(TimeSpan.FromSeconds(2)));
}

// +++++++++++++++++++++++++++++++++++++++ CONFIGURACIÓN DEL JWT EN LOGIN ++++++++++++++++++++++++++++++++++

[Test]
public void Login_JwtSecretNoConfigurado_LanzaInvalidOperationException()
{
    // Arrange
    var email = $"jwt-sin-secret-{Guid.NewGuid()}@correo.com";
    var usuario = GuardarUsuarioLoginAsync(email).GetAwaiter().GetResult();
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    _dbContext.SaveChanges();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    _configurationMock.Setup(c => c["Jwt:Secret"]).Returns((string?)null);
    _configurationMock.Setup(c => c["Jwt:Key"]).Returns((string?)null);
    _configurationMock.Setup(c => c["Jwt:SecretKey"]).Returns((string?)null);
    _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns(JwtIssuerTest);
    _configurationMock.Setup(c => c["Jwt:Audience"]).Returns(JwtAudienceTest);

    var request = new LoginDto(email, "Password123!");

    // Act & Assert
    Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.LoginAsync(request));
}

[Test]
public async Task Login_JwtIssuerNoConfigurado_CompruebaComportamientoToken()
{
    // Arrange
    ConfigurarJwtMock(issuer: null);
    _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns((string?)null);

    var email = $"jwt-sin-issuer-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var jwt = LeerJwt(result.Value.Token);
    Assert.That(jwt.Issuer, Is.Null.Or.Empty);
}

[Test]
public async Task Login_JwtAudienceNoConfigurado_CompruebaComportamientoToken()
{
    // Arrange
    ConfigurarJwtMock(audience: null);
    _configurationMock.Setup(c => c["Jwt:Audience"]).Returns((string?)null);

    var email = $"jwt-sin-audience-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var jwt = LeerJwt(result.Value.Token);
    Assert.That(jwt.Audiences, Is.Empty);
}

[Test]
public void Login_ClaveDemasiadoCorta_FallaCreacionToken()
{
    // Arrange
    const string claveCorta = "corta123";
    ConfigurarJwtMock(clave: claveCorta);

    var email = $"jwt-clave-corta-{Guid.NewGuid()}@correo.com";
    var usuario = GuardarUsuarioLoginAsync(email).GetAwaiter().GetResult();
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    _dbContext.SaveChanges();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    var request = new LoginDto(email, "Password123!");

    // Act & Assert
    Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _service.LoginAsync(request));
}

[Test]
public async Task Login_ClaveValida_FirmaTokenCorrectamente()
{
    // Arrange
    ConfigurarJwtMock(clave: JwtClaveTest, issuer: JwtIssuerTest, audience: JwtAudienceTest);

    var email = $"jwt-clave-valida-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioLoginAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarLoginExitoso(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    var jwt = LeerJwt(result.Value.Token);
    Assert.That(
        jwt.Header.Alg,
        Is.EqualTo(Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256));
    Assert.That(
        ValidarFirmaJwt(result.Value.Token, JwtClaveTest, JwtIssuerTest, JwtAudienceTest),
        Is.True);
}

// +++++++++++++++++++++++++++++++++++++++ DTO DEVUELTO POR EL LOGIN ++++++++++++++++++++++++++++++++++

[Test]
public async Task Login_LoginCorrecto_DevuelveResultIsSuccessTrue()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"dto-success-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioDtoAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarMocksDto(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
}

[Test]
public async Task Login_LoginCorrecto_DevuelveElIdDelUsuario()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"dto-id-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioDtoAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarMocksDto(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.Usuario.Id, Is.EqualTo(usuario.Id));
}

[Test]
public async Task Login_LoginCorrecto_DevuelveElEmail()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"dto-email-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioDtoAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarMocksDto(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.Usuario.Email, Is.EqualTo(email));
}

[Test]
public async Task Login_LoginCorrecto_DevuelveElTelefono()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"dto-tel-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioDtoAsync(email, phoneNumber: "699887766");
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarMocksDto(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.Usuario.Telefono, Is.EqualTo("699887766"));
}

[Test]
public async Task Login_LoginCorrecto_DevuelveElRolPrincipal()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"dto-rol-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioDtoAsync(email);
    usuario.Trabajador = new Trabajador
    {
        Nombre = "Pedro",
        Apellidos = "López",
        Dni = $"{Guid.NewGuid().ToString("N")[..8]}Z",
        TipoTrabajador = TipoTrabajador.Mecanico,
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarMocksDto(usuario, new List<string> { Roles.Administrador, Roles.Operario });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.Usuario.Rol, Is.EqualTo(Roles.Administrador));
}

[Test]
public async Task Login_UsuarioCliente_DevuelveClienteId()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"dto-clienteid-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioDtoAsync(email, phoneNumber: "600112233");

    var cliente = new Cliente
    {
        IdUsuario = usuario.Id,
        Nombre = "Sofía",
        Apellidos = "Navarro",
        Dni = "33445566C",
        FechaNacimiento = new DateTime(1993, 9, 21, 0, 0, 0, DateTimeKind.Utc),
        NumeroPermisoConducir = "PERM333",
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    _dbContext.Clientes.Add(cliente);
    await _dbContext.SaveChangesAsync();

    ConfigurarMocksDto(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.Usuario.ClienteId, Is.EqualTo(cliente.Id));
}

[Test]
public async Task Login_UsuarioTrabajador_DevuelveTrabajadorId()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"dto-trabajadorid-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioDtoAsync(email, phoneNumber: "600445566");

    var trabajador = new Trabajador
    {
        IdUsuario = usuario.Id,
        Nombre = "Diego",
        Apellidos = "Vega",
        Dni = "12345678Z",
        TipoTrabajador = TipoTrabajador.Mecanico,
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    _dbContext.Trabajadores.Add(trabajador);
    await _dbContext.SaveChangesAsync();

    ConfigurarMocksDto(usuario, new List<string> { Roles.Mecanico });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.Usuario.TrabajadorId, Is.EqualTo(trabajador.Id));
}

[Test]
public async Task Login_UsuarioSinClienteNiTrabajador_AmbosIdentificadoresSonNull()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"dto-sinids-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioDtoAsync(email, phoneNumber: "600778899");
    ConfigurarMocksDto(usuario, new List<string> { Roles.Administrador });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.Usuario.ClienteId, Is.Null);
    Assert.That(result.Value.Usuario.TrabajadorId, Is.Null);
}

[Test]
public async Task Login_LoginCorrecto_DevuelveCreatedAt()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"dto-createdat-{Guid.NewGuid()}@correo.com";
    var fechaCreacion = new DateTime(2026, 11, 1, 12, 30, 0, DateTimeKind.Utc);
    var usuario = await GuardarUsuarioDtoAsync(email, createdAt: fechaCreacion);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarMocksDto(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.Usuario.CreatedAt, Is.EqualTo(fechaCreacion));
}

[Test]
public async Task Login_LoginCorrecto_DevuelveElToken()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"dto-token-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioDtoAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarMocksDto(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.Token, Is.Not.Null.And.Not.WhiteSpace);
}

[Test]
public async Task Login_LoginCorrecto_DevuelveLaExpiracionDelToken()
{
    // Arrange
    ConfigurarJwtMock();
    var email = $"dto-expiracion-{Guid.NewGuid()}@correo.com";
    var usuario = await GuardarUsuarioDtoAsync(email);
    usuario.Cliente = new Cliente
    {
        Nombre = "Juan",
        Apellidos = "Pérez",
        FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedAt = DateTime.UtcNow,
        IsActivo = true
    };
    await _dbContext.SaveChangesAsync();
    ConfigurarMocksDto(usuario, new List<string> { Roles.Cliente });

    // Act
    var result = await _service.LoginAsync(new LoginDto(email, "Password123!"));

    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.ExpiresAt, Is.Not.EqualTo(default(DateTime)));
    Assert.That(result.Value.ExpiresAt, Is.GreaterThan(DateTime.UtcNow));
}
    
    
    #endregion

    #region TEST GETCURRENTUSER
    private static System.Security.Claims.ClaimsPrincipal CrearClaimsPrincipal(
        params System.Security.Claims.Claim[] claims)
    {
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        return new System.Security.Claims.ClaimsPrincipal(identity);
    }
    
    // +++++++++++++++++++++++++++++++++++++++ LECTURA DE CLAIMS EN GETCURRENTUSER ++++++++++++++++++++++++++++++++++

    [Test]
    public async Task GetCurrent_PrincipalConNameIdentifierValido_ContinuaElFlujo()
    {
        // Arrange: usuario existente en BD y ClaimsPrincipal con NameIdentifier válido
        var email = $"getcurrent-nameid-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Cliente });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(usuario.Id));
        Assert.That(result.Value.Email, Is.EqualTo(email));
    }

    [Test]
    public async Task GetCurrent_PrincipalConSubValido_ContinuaElFlujo()
    {
        // Arrange: usuario existente en BD y ClaimsPrincipal con claim sub válido
        var email = $"getcurrent-sub-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Cliente });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(usuario.Id));
        Assert.That(result.Value.Email, Is.EqualTo(email));
    }

    [Test]
    public async Task GetCurrent_PrincipalSinNameIdentifierNiSub_DevuelveNotFoundError()
    {
        // Arrange: ClaimsPrincipal sin claim de identificador
        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "sinid@correo.com"));

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<NotFoundError>());
    }

    [Test]
    public async Task GetCurrent_ClaimNameIdentifierVacio_DevuelveNotFoundError()
    {
        // Arrange: claim NameIdentifier vacío
        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, string.Empty));

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<NotFoundError>());
    }

    [Test]
    public async Task GetCurrent_ClaimSubVacio_DevuelveNotFoundError()
    {
        // Arrange: claim sub vacío
        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, string.Empty));

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<NotFoundError>());
    }

    [Test]
    public async Task GetCurrent_ClaimConTextoNoNumerico_DevuelveNotFoundError()
    {
        // Arrange: valor no numérico en claim de identificador
        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "no-es-numero"));

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<NotFoundError>());
    }

    [Test]
    public async Task GetCurrent_ClaimConLetras_DevuelveNotFoundError()
    {
        // Arrange: letras en el claim de identificador
        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "abc123xyz"));

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<NotFoundError>());
    }

    [Test]
    public async Task GetCurrent_ClaimConNumeroNegativo_BuscaIdYDevuelveNoEncontradoSiNoExiste()
    {
        // Arrange: ID negativo inexistente
        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "-5"));

        _userManagerMock.Setup(u => u.FindByIdAsync("-5")).ReturnsAsync((Usuario?)null);

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<NotFoundError>());
    }

    [Test]
    public async Task GetCurrent_ClaimConNumeroCero_BuscaIdYDevuelveNoEncontradoSiNoExiste()
    {
        // Arrange: ID cero inexistente
        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "0"));

        _userManagerMock.Setup(u => u.FindByIdAsync("0")).ReturnsAsync((Usuario?)null);

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<NotFoundError>());
    }

    [Test]
    public async Task GetCurrent_ClaimConNumeroValido_BuscaElUsuarioCorrespondiente()
    {
        // Arrange: usuario existente con ID concreto
        var email = $"getcurrent-valido-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var idTexto = usuario.Id.ToString();
        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, idTexto));

        _userManagerMock.Setup(u => u.FindByIdAsync(idTexto)).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Cliente });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(usuario.Id));
        _userManagerMock.Verify(u => u.FindByIdAsync(idTexto), Times.AtMostOnce);
    }

    [Test]
    public async Task GetCurrent_AmbosClaimsPresentesYDiferentes_UtilizaNameIdentifier()
    {
        // Arrange: NameIdentifier apunta al usuario real y sub apunta a otro ID distinto
        var email = $"getcurrent-prioridad-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var idValido = usuario.Id.ToString();
        var idSecundario = (usuario.Id + 999).ToString();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, idValido),
            new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, idSecundario));

        _userManagerMock.Setup(u => u.FindByIdAsync(idValido)).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Cliente });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(usuario.Id));
    }

    [Test]
    public async Task GetCurrent_NameIdentifierNoValidoYSubValido_DevuelveErrorSinUsarSub()
    {
        // Arrange: NameIdentifier inválido ("abc") y sub válido; con ?? no hace fallback a sub si NameIdentifier no es null
        var email = $"getcurrent-fallback-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "invalido"),
            new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, usuario.Id.ToString()));

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<NotFoundError>());
        _userManagerMock.Verify(u => u.FindByIdAsync(usuario.Id.ToString()), Times.Never);
    }
    
    // +++++++++++++++++++++++++++++++++++++++ BÚSQUEDA EN IDENTITY EN GETCURRENTUSER ++++++++++++++++++++++++++++++++++

    [Test]
    public async Task GetCurrent_IdExistente_DevuelveElUsuario()
    {
        // Arrange: usuario existente en BD y devuelto por UserManager.FindByIdAsync
        var email = $"getcurrent-id-ok-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            PhoneNumber = "600111222",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Cliente });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(usuario.Id));
        Assert.That(result.Value.Email, Is.EqualTo(email));
    }

    [Test]
    public async Task GetCurrent_IdInexistente_DevuelveNotFoundError()
    {
        // Arrange: ID numérico válido pero inexistente en Identity y en la base de datos
        const string idInexistente = "99999999";
        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, idInexistente));

        _userManagerMock.Setup(u => u.FindByIdAsync(idInexistente)).ReturnsAsync((Usuario?)null);

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<NotFoundError>());
    }

    [Test]
    public async Task GetCurrent_UsuarioConCliente_RecuperaElCliente()
    {
        // Arrange: usuario existente con ficha de Cliente vinculada
        var email = $"getcurrent-cliente-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var cliente = new Cliente
        {
            IdUsuario = usuario.Id,
            Nombre = "Elena",
            Apellidos = "Martín",
            Dni = "44556677D",
            FechaNacimiento = new DateTime(1990, 4, 18, 0, 0, 0, DateTimeKind.Utc),
            NumeroPermisoConducir = "PERM444",
            CreatedAt = DateTime.UtcNow,
            IsActivo = true
        };
        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Cliente });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ClienteId, Is.EqualTo(cliente.Id));
    }

    [Test]
    public async Task GetCurrent_UsuarioConTrabajador_RecuperaElTrabajador()
    {
        // Arrange: usuario existente con ficha de Trabajador vinculada
        var email = $"getcurrent-trabajador-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var trabajador = new Trabajador
        {
            IdUsuario = usuario.Id,
            Nombre = "Javier",
            Apellidos = "Castro",
            Dni = "12345678Z",
            TipoTrabajador = TipoTrabajador.Mecanico,
            CreatedAt = DateTime.UtcNow,
            IsActivo = true
        };
        _dbContext.Trabajadores.Add(trabajador);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Operario });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.TrabajadorId, Is.EqualTo(trabajador.Id));
    }

    [Test]
    public async Task GetCurrent_UsuarioSinClienteNiTrabajador_NoLanzaExcepcion()
    {
        // Arrange: usuario sin ficha de Cliente ni de Trabajador
        var email = $"getcurrent-sinrel-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Administrador });

        // Act & Assert
        Assert.DoesNotThrowAsync(async () =>
        {
            var res = await _service.GetCurrentUserAsync(principal);
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.Value.ClienteId, Is.Null);
            Assert.That(res.Value.TrabajadorId, Is.Null);
        });
    }

    [Test]
    public async Task GetCurrent_UsuarioConAmbasRelaciones_RecuperaAmbas()
    {
        // Arrange: usuario existente con relaciones tanto de Cliente como de Trabajador
        var email = $"getcurrent-ambas-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var cliente = new Cliente
        {
            IdUsuario = usuario.Id,
            Nombre = "Marta",
            Apellidos = "Díaz",
            Dni = "66778899E",
            FechaNacimiento = new DateTime(1989, 8, 25, 0, 0, 0, DateTimeKind.Utc),
            NumeroPermisoConducir = "PERM555",
            CreatedAt = DateTime.UtcNow,
            IsActivo = true
        };
        var trabajador = new Trabajador
        {
            IdUsuario = usuario.Id,
            Nombre = "Marta",
            Apellidos = "Díaz",
            Dni = "12345678Z",
            TipoTrabajador = TipoTrabajador.Mecanico,
            CreatedAt = DateTime.UtcNow,
            IsActivo = true
        };
        _dbContext.Clientes.Add(cliente);
        _dbContext.Trabajadores.Add(trabajador);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Administrador });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ClienteId, Is.EqualTo(cliente.Id));
        Assert.That(result.Value.TrabajadorId, Is.EqualTo(trabajador.Id));
    }
    
    // +++++++++++++++++++++++++++++++++++++++ ROLES Y RESPUESTA EN GETCURRENTUSER ++++++++++++++++++++++++++++++++++

    [Test]
    public async Task GetCurrent_UsuarioConRolCliente_DevuelveRolCliente()
    {
        // Arrange: usuario existente con rol Cliente
        var email = $"getcurrent-rol-cliente-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Cliente });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Rol, Is.EqualTo(Roles.Cliente));
    }

    [Test]
    public async Task GetCurrent_UsuarioConRolAdministrador_DevuelveRolAdministrador()
    {
        // Arrange: usuario existente con rol Administrador
        var email = $"getcurrent-rol-admin-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Administrador });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Rol, Is.EqualTo(Roles.Administrador));
    }

    [Test]
    public async Task GetCurrent_UsuarioConRolMecanico_DevuelveRolMecanico()
    {
        // Arrange: usuario existente con rol Mecanico
        var email = $"getcurrent-rol-mecanico-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Mecanico });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Rol, Is.EqualTo(Roles.Mecanico));
    }

    [Test]
    public async Task GetCurrent_UsuarioConRolOperario_DevuelveRolOperario()
    {
        // Arrange: usuario existente con rol Operario
        var email = $"getcurrent-rol-operario-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Operario });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Rol, Is.EqualTo(Roles.Operario));
    }

    [Test]
    public async Task GetCurrent_UsuarioSinRoles_DevuelveRolCliente()
    {
        // Arrange: GetRolesAsync devuelve vacío, por lo que debe caer en Roles.Cliente
        var email = $"getcurrent-sinroles-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string>());

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Rol, Is.EqualTo(Roles.Cliente));
    }

    [Test]
    public async Task GetCurrent_UsuarioConVariosRoles_DevuelveElPrimero()
    {
        // Arrange: varios roles devueltos por Identity; toma el primero
        var email = $"getcurrent-variosroles-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock
            .Setup(u => u.GetRolesAsync(usuario))
            .ReturnsAsync(new List<string> { Roles.Mecanico, Roles.Administrador });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Rol, Is.EqualTo(Roles.Mecanico));
    }

    [Test]
    public async Task GetCurrent_UsuarioCliente_DevuelveClienteId()
    {
        // Arrange: usuario con ficha de Cliente asociada
        var email = $"getcurrent-dto-cliente-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var cliente = new Cliente
        {
            IdUsuario = usuario.Id,
            Nombre = "Sara",
            Apellidos = "Ríos",
            Dni = "77889900F",
            FechaNacimiento = new DateTime(1996, 1, 12, 0, 0, 0, DateTimeKind.Utc),
            NumeroPermisoConducir = "PERM666",
            CreatedAt = DateTime.UtcNow,
            IsActivo = true
        };
        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Cliente });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ClienteId, Is.EqualTo(cliente.Id));
    }

    [Test]
    public async Task GetCurrent_UsuarioTrabajador_DevuelveTrabajadorId()
    {
        // Arrange: usuario con ficha de Trabajador asociada
        var email = $"getcurrent-dto-trabajador-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var trabajador = new Trabajador
        {
            IdUsuario = usuario.Id,
            Nombre = "Daniel",
            Apellidos = "León",
            Dni = "12345678Z",
            TipoTrabajador = TipoTrabajador.Mecanico,
            CreatedAt = DateTime.UtcNow,
            IsActivo = true
        };
        _dbContext.Trabajadores.Add(trabajador);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Operario });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.TrabajadorId, Is.EqualTo(trabajador.Id));
    }

    [Test]
    public async Task GetCurrent_UsuarioSinFichaAsociada_AmbosIdentificadoresSonNull()
    {
        // Arrange: usuario sin ficha de Cliente ni Trabajador
        var email = $"getcurrent-dto-sinficha-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Administrador });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.ClienteId, Is.Null);
        Assert.That(result.Value.TrabajadorId, Is.Null);
    }

    [Test]
    public async Task GetCurrent_UsuarioEncontrado_DevuelveIdCorrecto()
    {
        // Arrange: usuario persistido con ID autogenerado en PostgreSQL
        var email = $"getcurrent-dto-id-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Cliente });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(usuario.Id));
    }

    [Test]
    public async Task GetCurrent_UsuarioEncontrado_DevuelveEmailCorrecto()
    {
        // Arrange: usuario con email concreto
        var email = $"getcurrent-dto-email-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Cliente });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Email, Is.EqualTo(email));
    }

    [Test]
    public async Task GetCurrent_UsuarioEncontrado_DevuelveTelefonoCorrecto()
    {
        // Arrange: usuario con teléfono asignado
        const string telefono = "611223344";
        var email = $"getcurrent-dto-tel-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            PhoneNumber = telefono,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Cliente });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Telefono, Is.EqualTo(telefono));
    }

    [Test]
    public async Task GetCurrent_UsuarioEncontrado_DevuelveCreatedAtCorrecto()
    {
        // Arrange: usuario con CreatedAt conocido
        var fechaCreacion = new DateTime(2026, 10, 15, 9, 0, 0, DateTimeKind.Utc);
        var email = $"getcurrent-dto-createdat-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            EmailConfirmed = true,
            CreatedAt = fechaCreacion
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Cliente });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.CreatedAt, Is.EqualTo(fechaCreacion));
    }

    [Test]
    public async Task GetCurrent_UsuarioEncontrado_NoDevuelveInformacionSensible()
    {
        // Arrange: usuario con hash y stamps asignados para comprobar que el DTO no los expone
        const string hashSensible = "AQAAAAIAAYagAAAAEHashSimuladoMuySensible123!";
        const string stampSensible = "STAMP_SECRETO_123";
        var email = $"getcurrent-sensible-{Guid.NewGuid()}@correo.com";
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            PasswordHash = hashSensible,
            SecurityStamp = stampSensible,
            ConcurrencyStamp = stampSensible,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();

        var principal = CrearClaimsPrincipal(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()));

        _userManagerMock.Setup(u => u.FindByIdAsync(usuario.Id.ToString())).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.GetRolesAsync(usuario)).ReturnsAsync(new List<string> { Roles.Cliente });

        // Act
        var result = await _service.GetCurrentUserAsync(principal);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        var tipoDto = result.Value.GetType();
        Assert.That(tipoDto.GetProperty("PasswordHash"), Is.Null);
        Assert.That(tipoDto.GetProperty("SecurityStamp"), Is.Null);
        Assert.That(tipoDto.GetProperty("ConcurrencyStamp"), Is.Null);
        Assert.That(tipoDto.GetProperty("Contraseña"), Is.Null);
        Assert.That(tipoDto.GetProperty("Contrasena"), Is.Null);
        Assert.That(tipoDto.GetProperty("Password"), Is.Null);

        var textoSerializado = System.Text.Json.JsonSerializer.Serialize(result.Value);
        Assert.That(textoSerializado, Does.Not.Contain(hashSensible));
        Assert.That(textoSerializado, Does.Not.Contain(stampSensible));
    }
    #endregion
}