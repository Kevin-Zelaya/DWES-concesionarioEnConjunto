using APIAlquilerVehiculos.Dtos.Trabajadores;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Repositories.Trabajadores;
using APIAlquilerVehiculos.Services.Trabajadores;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.PostgreSql;

namespace APIAlquilerVehiculos.Test.Services.Test.Trabajadores;

[TestFixture]
public class TrabajadorServiceTests
{
    private Mock<ITrabajadorRepository> _repositoryMock = null!;
    private Mock<IValidator<CreateTrabajadorDto>> _createValidatorMock = null!;
    private Mock<IValidator<UpdateTrabajadorDto>> _updateValidatorMock = null!;
    private Mock<ILogger<TrabajadorService>> _loggerMock = null!;
    private Mock<UserManager<Usuario>> _userManagerMock = null!;
    private Mock<ICache> _cacheMock = null!;
    private PostgreSqlContainer _postgresContainer = null!;
    private AppDbContext _dbContext = null!;
    private TrabajadorService _service = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("alquiler_test_db_trabajadores")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _postgresContainer.StartAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _postgresContainer.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        _repositoryMock = new Mock<ITrabajadorRepository>();
        _createValidatorMock = new Mock<IValidator<CreateTrabajadorDto>>();
        _updateValidatorMock = new Mock<IValidator<UpdateTrabajadorDto>>();
        _loggerMock = new Mock<ILogger<TrabajadorService>>();
        _cacheMock = new Mock<ICache>();

        var store = new Mock<IUserStore<Usuario>>();
        _userManagerMock = new Mock<UserManager<Usuario>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        _dbContext = new AppDbContext(options);
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();

        _service = new TrabajadorService(
            _repositoryMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            _userManagerMock.Object,
            _dbContext,
            _cacheMock.Object,
            _loggerMock.Object
        );
    }

    [TearDown]
    public async Task TearDown()
    {
        await _dbContext.DisposeAsync();
    }

    #region Helpers

    private static Trabajador CrearTrabajador(
        long id = 1,
        long idUsuario = 10,
        bool activo = true,
        TipoTrabajador tipo = TipoTrabajador.Operaciones,
        string dni = "11111111A")
    {
        return new Trabajador
        {
            Id = id,
            IdUsuario = idUsuario,
            Usuario = new Usuario
            {
                Id = idUsuario,
                Email = "t@test.com",
                UserName = "t@test.com",
                PhoneNumber = "600000000"
            },
            Nombre = "Juan",
            Apellidos = "Pérez",
            Dni = dni,
            FechaIngreso = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TipoTrabajador = tipo,
            IsActivo = activo,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static CreateTrabajadorDto CrearCreateDto(
        string email = "nuevo@test.com",
        string dni = "22222222B",
        TipoTrabajador tipo = TipoTrabajador.Operaciones) =>
        new(
            Email: email,
            Telefono: "600111222",
            Contraseña: "Password123",
            Nombre: "Nuevo",
            Apellidos: "Trabajador",
            Dni: dni,
            FechaIngreso: null,
            TipoTrabajador: tipo);

    private static UpdateTrabajadorDto CrearUpdateDto(
        string email = "t@test.com",
        string telefono = "600000000",
        string dni = "11111111A",
        TipoTrabajador tipo = TipoTrabajador.Operaciones) =>
        new(
            Email: email,
            Telefono: telefono,
            Nombre: "Juan Actualizado",
            Apellidos: "Pérez García",
            Dni: dni,
            FechaIngreso: new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TipoTrabajador: tipo);

    private void SetupValidacionCreateOk() =>
        _createValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateTrabajadorDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

    private void SetupValidacionUpdateOk() =>
        _updateValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateTrabajadorDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

    #endregion

    #region GetAllPagedAsync

    [Test]
    public async Task GetAllPagedAsync_Devuelve_Dtos_Paginados()
    {
        // Arrange
        var items = new List<Trabajador> { CrearTrabajador(1), CrearTrabajador(2, 20) };
        _repositoryMock
            .Setup(r => r.GetAllPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<TipoTrabajador?>()))
            .ReturnsAsync((items, 2));

        // Act
        var result = await _service.GetAllPagedAsync(0, 500);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Email.Should().Be("t@test.com");
    }

    #endregion

    #region GetByIdAsync

    [Test]
    public async Task GetByIdAsync_Devuelve_Desde_Cache_Sin_Llamar_Al_Repositorio()
    {
        // Arrange
        var dtoCacheado = new ResponseTrabajadorDto(1, 10, "Juan", "Pérez", "t@test.com", "600000000", "11111111A", DateTime.UtcNow, "Operaciones", true, DateTime.UtcNow, null);
        _cacheMock.Setup(c => c.GetTAsync<ResponseTrabajadorDto>("trabajador:1")).ReturnsAsync(dtoCacheado);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(dtoCacheado);
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Never);
    }

    [Test]
    public async Task GetByIdAsync_Devuelve_NotFound_Cuando_No_Esta_En_Cache_Ni_En_Repositorio()
    {
        // Arrange
        _cacheMock.Setup(c => c.GetTAsync<ResponseTrabajadorDto>("trabajador:5")).ReturnsAsync((ResponseTrabajadorDto?)null);
        _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Trabajador?)null);

        // Act
        var result = await _service.GetByIdAsync(5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Test]
    public async Task GetByIdAsync_Devuelve_Trabajador_Y_Guarda_En_Cache()
    {
        // Arrange
        var trabajador = CrearTrabajador(5, 50);
        _cacheMock.Setup(c => c.GetTAsync<ResponseTrabajadorDto>("trabajador:5")).ReturnsAsync((ResponseTrabajadorDto?)null);
        _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(trabajador);

        // Act
        var result = await _service.GetByIdAsync(5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(5);
        _cacheMock.Verify(c => c.SetAsync("trabajador:5", It.IsAny<ResponseTrabajadorDto>(), null), Times.Once);
    }

    #endregion

    #region AddAsync

    [Test]
    public async Task AddAsync_Devuelve_ValidationError_Cuando_Dto_Es_Invalido()
    {
        // Arrange
        _createValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateTrabajadorDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Nombre", "Obligatorio") }));

        // Act
        var result = await _service.AddAsync(CrearCreateDto());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
    }

    [Test]
    public async Task AddAsync_Devuelve_ConflictError_Cuando_Dni_Ya_Existe()
    {
        // Arrange
        SetupValidacionCreateOk();
        _repositoryMock.Setup(r => r.ExistsByDniAsync("22222222B")).ReturnsAsync(true);

        // Act
        var result = await _service.AddAsync(CrearCreateDto(dni: " 22222222b "));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
    }

    [Test]
    public async Task AddAsync_Devuelve_ConflictError_Cuando_Email_Ya_Existe()
    {
        // Arrange
        SetupValidacionCreateOk();
        _repositoryMock.Setup(r => r.ExistsByDniAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userManagerMock.Setup(u => u.FindByEmailAsync("nuevo@test.com"))
            .ReturnsAsync(new Usuario { Id = 99, Email = "nuevo@test.com" });

        // Act
        var result = await _service.AddAsync(CrearCreateDto());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
    }

    [Test]
    public async Task AddAsync_Devuelve_Error_Cuando_Identity_Falla_Al_Crear_Usuario()
    {
        // Arrange
        SetupValidacionCreateOk();
        _repositoryMock.Setup(r => r.ExistsByDniAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password débil" }));

        // Act
        var result = await _service.AddAsync(CrearCreateDto());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
    }

    [Test]
    public async Task AddAsync_Devuelve_InternalError_Cuando_Falla_Asignar_Rol()
    {
        // Arrange
        SetupValidacionCreateOk();
        _repositoryMock.Setup(r => r.ExistsByDniAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Rol inexistente" }));

        // Act
        var result = await _service.AddAsync(CrearCreateDto());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<InternalError>();
    }

    [Test]
    public async Task AddAsync_Devuelve_Trabajador_Creado_Y_Guarda_En_Cache()
    {
        // Arrange
        SetupValidacionCreateOk();
        _repositoryMock.Setup(r => r.ExistsByDniAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var creado = CrearTrabajador(1, 0, dni: "22222222B");
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Trabajador>())).ReturnsAsync(creado);

        // Act
        var result = await _service.AddAsync(CrearCreateDto());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1);
        _cacheMock.Verify(c => c.SetAsync("trabajador:1", It.IsAny<ResponseTrabajadorDto>(), null), Times.Once);
    }

    [Test]
    public async Task AddAsync_Devuelve_ConflictError_Cuando_Lanza_DbUpdateException()
    {
        // Arrange
        SetupValidacionCreateOk();
        _repositoryMock.Setup(r => r.ExistsByDniAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Trabajador>()))
            .ThrowsAsync(new DbUpdateException("Unique constraint"));

        // Act
        var result = await _service.AddAsync(CrearCreateDto());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
    }

    [Test]
    public async Task AddAsync_Devuelve_InternalError_Cuando_Lanza_Excepcion_Inesperada()
    {
        // Arrange
        SetupValidacionCreateOk();
        _repositoryMock.Setup(r => r.ExistsByDniAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<Usuario>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Trabajador>()))
            .ThrowsAsync(new InvalidOperationException("Fallo inesperado"));

        // Act
        var result = await _service.AddAsync(CrearCreateDto());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<InternalError>();
    }

    #endregion

    #region UpdateAsync

    [Test]
    public async Task UpdateAsync_Devuelve_ValidationError_Cuando_Dto_Es_Invalido()
    {
        // Arrange
        _updateValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateTrabajadorDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Email", "Obligatorio") }));

        // Act
        var result = await _service.UpdateAsync(1, CrearUpdateDto());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
    }

    [Test]
    public async Task UpdateAsync_Devuelve_NotFound_Cuando_Trabajador_No_Existe()
    {
        // Arrange
        SetupValidacionUpdateOk();
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Trabajador?)null);

        // Act
        var result = await _service.UpdateAsync(99, CrearUpdateDto());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Test]
    public async Task UpdateAsync_Devuelve_ConflictError_Cuando_Dni_Cambiado_Ya_Existe()
    {
        // Arrange
        SetupValidacionUpdateOk();
        var actual = CrearTrabajador(1, 10, dni: "11111111A");
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(actual);
        _repositoryMock.Setup(r => r.ExistsByDniAsync("22222222B")).ReturnsAsync(true);

        // Act
        var result = await _service.UpdateAsync(1, CrearUpdateDto(dni: "22222222B"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
    }

    [Test]
    public async Task UpdateAsync_Devuelve_ConflictError_Cuando_Email_Pertenece_A_Otro_Usuario()
    {
        // Arrange
        SetupValidacionUpdateOk();
        var actual = CrearTrabajador(1, 10);
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(actual);
        _userManagerMock.Setup(u => u.FindByEmailAsync("ocupado@test.com"))
            .ReturnsAsync(new Usuario { Id = 99, Email = "ocupado@test.com" });

        // Act
        var result = await _service.UpdateAsync(1, CrearUpdateDto(email: "ocupado@test.com"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
    }

    [Test]
    public async Task UpdateAsync_Devuelve_NotFound_Cuando_Repositorio_Devuelve_Null()
    {
        // Arrange
        SetupValidacionUpdateOk();
        var actual = CrearTrabajador(1, 10);
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(actual);
        _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Trabajador>())).ReturnsAsync((Trabajador?)null);

        // Act
        var result = await _service.UpdateAsync(1, CrearUpdateDto());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_Devuelve_Error_Cuando_No_Existe_El_Usuario_Asociado()
    {
        // Arrange
        SetupValidacionUpdateOk();
        var actual = CrearTrabajador(1, 10);
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(actual);
        _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Trabajador>())).ReturnsAsync(actual);
        _userManagerMock.Setup(u => u.FindByIdAsync("10")).ReturnsAsync((Usuario?)null);

        // Act
        var result = await _service.UpdateAsync(1, CrearUpdateDto());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_Devuelve_Error_Cuando_Falla_SetEmail()
    {
        // Arrange
        SetupValidacionUpdateOk();
        var actual = CrearTrabajador(1, 10);
        var usuario = new Usuario { Id = 10, Email = "viejo@test.com", UserName = "viejo@test.com", PhoneNumber = "600000000" };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(actual);
        _userManagerMock.Setup(u => u.FindByEmailAsync("nuevo@test.com")).ReturnsAsync((Usuario?)null);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Trabajador>())).ReturnsAsync(actual);
        _userManagerMock.Setup(u => u.FindByIdAsync("10")).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.SetEmailAsync(usuario, "nuevo@test.com"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email en uso" }));

        // Act
        var result = await _service.UpdateAsync(1, CrearUpdateDto(email: "nuevo@test.com"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_Devuelve_Trabajador_Y_Limpia_Cache_Cuando_No_Cambia_Nada_En_Identity()
    {
        // Arrange
        SetupValidacionUpdateOk();
        var actual = CrearTrabajador(1, 10);
        // email igual ignorando mayúsculas, teléfono igual y mismo tipo -> no toca Identity salvo UpdateAsync
        var usuario = new Usuario { Id = 10, Email = "T@TEST.COM", UserName = "t@test.com", PhoneNumber = "600000000" };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(actual);
        _userManagerMock.Setup(u => u.FindByEmailAsync("t@test.com")).ReturnsAsync(usuario);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Trabajador>())).ReturnsAsync(actual);
        _userManagerMock.Setup(u => u.FindByIdAsync("10")).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.UpdateAsync(usuario)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.UpdateAsync(1, CrearUpdateDto());

        // Assert
        result.IsSuccess.Should().BeTrue();
        _cacheMock.Verify(c => c.RemoveAsync("trabajador:1"), Times.Once);
        _userManagerMock.Verify(u => u.SetEmailAsync(It.IsAny<Usuario>(), It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(u => u.SetPhoneNumberAsync(It.IsAny<Usuario>(), It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(u => u.RemoveFromRoleAsync(It.IsAny<Usuario>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_Sincroniza_Email_Telefono_Y_Rol_Cuando_Cambian()
    {
        // Arrange
        SetupValidacionUpdateOk();
        var actual = CrearTrabajador(1, 10, tipo: TipoTrabajador.Operaciones, dni: "11111111A");
        var usuario = new Usuario { Id = 10, Email = "viejo@test.com", UserName = "viejo@test.com", PhoneNumber = "111" };

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(actual);
        _repositoryMock.Setup(r => r.ExistsByDniAsync("22222222B")).ReturnsAsync(false);
        _userManagerMock.Setup(u => u.FindByEmailAsync("nuevo@test.com")).ReturnsAsync((Usuario?)null);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Trabajador>())).ReturnsAsync(actual);
        _userManagerMock.Setup(u => u.FindByIdAsync("10")).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.SetEmailAsync(usuario, "nuevo@test.com")).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.SetUserNameAsync(usuario, "nuevo@test.com")).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.SetPhoneNumberAsync(usuario, "222")).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.IsInRoleAsync(usuario, Roles.Operario)).ReturnsAsync(true);
        _userManagerMock.Setup(u => u.RemoveFromRoleAsync(usuario, Roles.Operario)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(usuario, Roles.Mecanico)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.UpdateAsync(usuario)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.UpdateAsync(1, CrearUpdateDto(
            email: "nuevo@test.com",
            telefono: "222",
            dni: "22222222B",
            tipo: TipoTrabajador.Mecanico));

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(u => u.SetEmailAsync(usuario, "nuevo@test.com"), Times.Once);
        _userManagerMock.Verify(u => u.SetPhoneNumberAsync(usuario, "222"), Times.Once);
        _userManagerMock.Verify(u => u.RemoveFromRoleAsync(usuario, Roles.Operario), Times.Once);
        _userManagerMock.Verify(u => u.AddToRoleAsync(usuario, Roles.Mecanico), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_Devuelve_ConflictError_Cuando_Lanza_DbUpdateException()
    {
        // Arrange
        SetupValidacionUpdateOk();
        var actual = CrearTrabajador(1, 10);
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(actual);
        _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Trabajador>()))
            .ThrowsAsync(new DbUpdateException("Unique constraint"));

        // Act
        var result = await _service.UpdateAsync(1, CrearUpdateDto());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
    }

    [Test]
    public async Task UpdateAsync_Devuelve_InternalError_Cuando_Lanza_Excepcion_Inesperada()
    {
        // Arrange
        SetupValidacionUpdateOk();
        var actual = CrearTrabajador(1, 10);
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(actual);
        _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Trabajador>()))
            .ThrowsAsync(new InvalidOperationException("Fallo inesperado"));

        // Act
        var result = await _service.UpdateAsync(1, CrearUpdateDto());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<InternalError>();
    }

    #endregion

    #region DeleteAsync

    [Test]
    public async Task DeleteAsync_Devuelve_NotFound_Cuando_Trabajador_No_Existe()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Trabajador?)null);

        // Act
        var result = await _service.DeleteAsync(99);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Test]
    public async Task DeleteAsync_Devuelve_ConflictError_Cuando_Ya_Esta_Inactivo()
    {
        // Arrange
        var inactivo = CrearTrabajador(1, 10, activo: false);
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inactivo);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
    }

    [Test]
    public async Task DeleteAsync_Devuelve_ConflictError_Cuando_Es_Responsable_De_Oficina()
    {
        // Arrange
        var activo = CrearTrabajador(1, 10);
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activo);
        _repositoryMock.Setup(r => r.EsResponsableAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<long>()), Times.Never);
    }

    [Test]
    public async Task DeleteAsync_Devuelve_ConflictError_Cuando_Tiene_Reservas_Vivas()
    {
        // Arrange
        var activo = CrearTrabajador(1, 10);
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activo);
        _repositoryMock.Setup(r => r.EsResponsableAsync(1)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.TieneReservasVivasAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<long>()), Times.Never);
    }

    [Test]
    public async Task DeleteAsync_Devuelve_NotFound_Cuando_Repositorio_Devuelve_False()
    {
        // Arrange
        var activo = CrearTrabajador(1, 10);
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activo);
        _repositoryMock.Setup(r => r.EsResponsableAsync(1)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.TieneReservasVivasAsync(1)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();

        _cacheMock.Verify(c => c.RemoveAsync("trabajador:1"), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_Devuelve_Success_Y_Limpia_Cache()
    {
        // Arrange
        var activo = CrearTrabajador(1, 10);
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activo);
        _repositoryMock.Setup(r => r.EsResponsableAsync(1)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.TieneReservasVivasAsync(1)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _cacheMock.Verify(c => c.RemoveAsync("trabajador:1"), Times.Once);
    }

    #endregion
}
