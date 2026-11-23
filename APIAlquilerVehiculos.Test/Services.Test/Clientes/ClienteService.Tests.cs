using System.Security.Claims;
using APIAlquilerVehiculos.Dtos.Cientes;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Repositories.Clientes;
using APIAlquilerVehiculos.Services.Clientes;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using Testcontainers.PostgreSql;

namespace APIAlquilerVehiculos.Test.Services.Test.Clientes;

[TestFixture]
public class ClienteServiceTests
{
    private Mock<IClienteRepository> _repositoryMock = null!;
    private Mock<IValidator<UpdateClienteDto>> _validatorMock = null!;
    private Mock<ILogger<ClienteService>> _loggerMock = null!;
    private Mock<UserManager<Usuario>> _userManagerMock = null!;
    private Mock<ICache> _cacheMock = null!;
    private PostgreSqlContainer _postgresContainer = null!;
    private AppDbContext _dbContext = null!;
    private ClienteService _service = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("alquiler_test_db")
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
        _repositoryMock = new Mock<IClienteRepository>();
        _validatorMock = new Mock<IValidator<UpdateClienteDto>>();
        _loggerMock = new Mock<ILogger<ClienteService>>();
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

        _userManagerMock.Setup(u => u.Users).Returns(_dbContext.Users);

        _service = new ClienteService(
            _repositoryMock.Object,
            _validatorMock.Object,
            _loggerMock.Object,
            _userManagerMock.Object,
            _dbContext,
            _cacheMock.Object
        );
    }

    [TearDown]
    public async Task TearDown()
    {
        await _dbContext.DisposeAsync();
    }

    #region Helpers

    private static ClaimsPrincipal CrearPrincipal(long userId, string claimType = ClaimTypes.NameIdentifier)
    {
        var claims = new[] { new Claim(claimType, userId.ToString()) };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private static ClaimsPrincipal CrearPrincipalSinId()
    {
        return new ClaimsPrincipal(new ClaimsIdentity([], "TestAuth"));
    }

    private static Cliente CrearClienteEntidad(long id = 1, long idUsuario = 10, bool isActivo = true, string? dni = "12345678Z")
    {
        return new Cliente
        {
            Id = id,
            IdUsuario = idUsuario,
            Nombre = "Juan",
            Apellidos = "Pérez",
            FechaNacimiento = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Dni = dni,
            IsActivo = isActivo,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static UpdateClienteDto CrearUpdateDto(long id = 1, string email = "nuevo@test.com", string telefono = "600111222", string? dni = "12345678Z")
    {
        return new UpdateClienteDto(
            Id: id,
            Nombre: "Juan Actualizado",
            Apellidos: "Pérez García",
            Email: email,
            Telefono: telefono,
            FechaNacimiento: new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Dni: dni,
            Nie: null,
            Pasaporte: null,
            NumeroPermisoConducir: null
        );
    }

    private async Task<Usuario> SembrarUsuarioEnBdAsync(long userId, string email = "user@test.com", Cliente? cliente = null)
    {
        var usuario = new Usuario
        {
            Id = userId,
            Email = email,
            UserName = email,
            PhoneNumber = "600000000",
            CreatedAt = DateTime.UtcNow,
            Cliente = cliente
        };
        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();
        return usuario;
    }

    #endregion

    #region FindAllPagedAsync

    [Test]
    public async Task FindAllPagedAsync_Devuelve_Lista_Paginada_Correctamente()
    {
        // Arrange
        var filter = new ClienteFilterDto { Page = 0, Size = 10 };
        var clientes = new List<Cliente> { CrearClienteEntidad(1, 10), CrearClienteEntidad(2, 20) };
        _repositoryMock
            .Setup(r => r.FindAllPagedAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync((clientes, 2));

        // Act
        var result = await _service.FindAllPagedAsync(filter, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }

    [Test]
    public async Task FindAllPagedAsync_Devuelve_Lista_Vacia_Cuando_No_Hay_Resultados()
    {
        // Arrange
        var filter = new ClienteFilterDto { Page = 1, Size = 5 };
        _repositoryMock
            .Setup(r => r.FindAllPagedAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Cliente>(), 0));

        // Act
        var result = await _service.FindAllPagedAsync(filter, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.Page.Should().Be(2);
    }

    #endregion

    #region GetByIdAsync

    [Test]
    public async Task GetByIdAsync_Devuelve_NotFound_Cuando_Id_Es_Menor_O_Igual_A_Cero()
    {
        // Arrange
        var principal = CrearPrincipal(10);

        // Act
        var result = await _service.GetByIdAsync(principal, 0, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
        _repositoryMock.Verify(r => r.FindByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetByIdAsync_Devuelve_UserNotFound_Cuando_Claim_No_Tiene_Id_Valido()
    {
        // Arrange
        var principal = CrearPrincipalSinId();

        // Act
        var result = await _service.GetByIdAsync(principal, 1, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Test]
    public async Task GetByIdAsync_Devuelve_UserNotFound_Cuando_Usuario_No_Existe_En_Bd()
    {
        // Arrange
        var principal = CrearPrincipal(999, JwtRegisteredClaimNames.Sub);

        // Act
        var result = await _service.GetByIdAsync(principal, 1, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Test]
    public async Task GetByIdAsync_Devuelve_Forbidden_Cuando_Cliente_Intenta_Consultar_Otro_Cliente()
    {
        // Arrange
        var clientePropio = CrearClienteEntidad(id: 1, idUsuario: 10);
        var usuario = await SembrarUsuarioEnBdAsync(10, cliente: clientePropio);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.Is<Usuario>(x => x.Id == 10)))
            .ReturnsAsync(new List<string> { Roles.Cliente });

        var principal = CrearPrincipal(10);

        // Act (busca cliente ID 2 siendo cliente ID 1)
        var result = await _service.GetByIdAsync(principal, 2, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ForbiddenError>();
    }

    [Test]
    public async Task GetByIdAsync_Devuelve_Cliente_Desde_Cache_Cuando_Existe()
    {
        // Arrange
        var clientePropio = CrearClienteEntidad(id: 1, idUsuario: 10);
        await SembrarUsuarioEnBdAsync(10, cliente: clientePropio);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.Is<Usuario>(x => x.Id == 10)))
            .ReturnsAsync(new List<string> { Roles.Cliente });

        var dtoCacheado = new ClienteDto(1, 10, "Juan", "Pérez", "juan@test.com", "600000000", DateTime.UtcNow, "12345678Z", null, null, null, DateTime.UtcNow, null, true, null);
        _cacheMock.Setup(c => c.GetTAsync<ClienteDto>("cliente:1")).ReturnsAsync(dtoCacheado);

        var principal = CrearPrincipal(10);

        // Act
        var result = await _service.GetByIdAsync(principal, 1, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(dtoCacheado);
        _repositoryMock.Verify(r => r.FindByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetByIdAsync_Devuelve_NotFound_Cuando_No_Esta_En_Cache_Ni_En_Repositorio()
    {
        // Arrange
        await SembrarUsuarioEnBdAsync(10);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.Is<Usuario>(x => x.Id == 10)))
            .ReturnsAsync(new List<string> { Roles.Administrador });

        _cacheMock.Setup(c => c.GetTAsync<ClienteDto>("cliente:5")).ReturnsAsync((ClienteDto?)null);
        _repositoryMock.Setup(r => r.FindByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

        var principal = CrearPrincipal(10);

        // Act
        var result = await _service.GetByIdAsync(principal, 5, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Test]
    public async Task GetByIdAsync_Devuelve_Cliente_Desde_Repositorio_Y_Guarda_En_Cache()
    {
        // Arrange
        await SembrarUsuarioEnBdAsync(10);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.Is<Usuario>(x => x.Id == 10)))
            .ReturnsAsync(new List<string> { Roles.Mecanico });

        var clienteBd = CrearClienteEntidad(id: 5, idUsuario: 50);
        _cacheMock.Setup(c => c.GetTAsync<ClienteDto>("cliente:5")).ReturnsAsync((ClienteDto?)null);
        _repositoryMock.Setup(r => r.FindByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(clienteBd);

        var principal = CrearPrincipal(10);

        // Act
        var result = await _service.GetByIdAsync(principal, 5, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(5);
        _cacheMock.Verify(c => c.SetAsync("cliente:5", It.IsAny<ClienteDto>(), null), Times.Once);
    }

    #endregion

    #region UpdateAsync

    [Test]
    public async Task UpdateAsync_Devuelve_NotFound_Cuando_Cliente_No_Existe()
    {
        // Arrange
        var dto = CrearUpdateDto(id: 99);
        _repositoryMock.Setup(r => r.FindByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

        // Act
        var result = await _service.UpdateAsync(CrearPrincipal(10), 99, dto, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Test]
    public async Task UpdateAsync_Devuelve_Forbidden_Cuando_Cliente_Intenta_Actualizar_Otro_Cliente()
    {
        // Arrange
        var clienteObjetivo = CrearClienteEntidad(id: 2, idUsuario: 20);
        var clientePropio = CrearClienteEntidad(id: 1, idUsuario: 10);
        await SembrarUsuarioEnBdAsync(10, cliente: clientePropio);

        _repositoryMock.Setup(r => r.FindByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(clienteObjetivo);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.Is<Usuario>(x => x.Id == 10)))
            .ReturnsAsync(new List<string> { Roles.Cliente });

        // Act
        var result = await _service.UpdateAsync(CrearPrincipal(10), 2, CrearUpdateDto(2), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ForbiddenError>();
    }

    [Test]
    public async Task UpdateAsync_Devuelve_ValidationError_Cuando_Dto_Es_Invalido()
    {
        // Arrange
        var clienteActual = CrearClienteEntidad(id: 1, idUsuario: 10);
        await SembrarUsuarioEnBdAsync(10, cliente: clienteActual);

        _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(clienteActual);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<Usuario>())).ReturnsAsync(new List<string> { Roles.Cliente });

        var validationFailures = new List<ValidationFailure> { new("Nombre", "El nombre es obligatorio") };
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateClienteDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var result = await _service.UpdateAsync(CrearPrincipal(10), 1, CrearUpdateDto(1), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
    }

    [Test]
    public async Task UpdateAsync_Devuelve_ConflictError_Cuando_Dni_Ya_Existe_En_Otro_Cliente()
    {
        // Arrange
        var clienteActual = CrearClienteEntidad(id: 1, idUsuario: 10, dni: "11111111A");
        var otroCliente = CrearClienteEntidad(id: 2, idUsuario: 20, dni: "87654321X");
        await SembrarUsuarioEnBdAsync(10, "u1@test.com", clienteActual);
        await SembrarUsuarioEnBdAsync(20, "u2@test.com", otroCliente);

        _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(clienteActual);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<Usuario>())).ReturnsAsync(new List<string> { Roles.Cliente });
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateClienteDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var dtoConDniDuplicado = CrearUpdateDto(id: 1, dni: " 87654321x "); // Prueba normalización trim + uppercase

        // Act
        var result = await _service.UpdateAsync(CrearPrincipal(10), 1, dtoConDniDuplicado, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
    }

    [Test]
    public async Task UpdateAsync_Devuelve_ConflictError_Cuando_Email_Ya_Existe_En_Otro_Usuario()
    {
        // Arrange
        var clienteActual = CrearClienteEntidad(id: 1, idUsuario: 10);
        await SembrarUsuarioEnBdAsync(10, "actual@test.com", clienteActual);
        var otroUsuario = new Usuario { Id = 99, Email = "ocupado@test.com" };

        _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(clienteActual);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<Usuario>())).ReturnsAsync(new List<string> { Roles.Cliente });
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateClienteDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _userManagerMock.Setup(u => u.FindByEmailAsync("ocupado@test.com")).ReturnsAsync(otroUsuario);

        var dto = CrearUpdateDto(id: 1, email: "ocupado@test.com");

        // Act
        var result = await _service.UpdateAsync(CrearPrincipal(10), 1, dto, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
    }

    [Test]
    public async Task UpdateAsync_Devuelve_NotFound_Y_Hace_Rollback_Cuando_Repositorio_Devuelve_Null()
    {
        // Arrange
        var clienteActual = CrearClienteEntidad(id: 1, idUsuario: 10);
        await SembrarUsuarioEnBdAsync(10, "actual@test.com", clienteActual);

        _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(clienteActual);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<Usuario>())).ReturnsAsync(new List<string> { Roles.Cliente });
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateClienteDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
        _repositoryMock.Setup(r => r.UpdateAsync(1, It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        // Act
        var result = await _service.UpdateAsync(CrearPrincipal(10), 1, CrearUpdateDto(1), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_Devuelve_Error_Y_Hace_Rollback_Cuando_Falla_Sincronizacion_Identity()
    {
        // Arrange
        var clienteActual = CrearClienteEntidad(id: 1, idUsuario: 10);
        var usuario = await SembrarUsuarioEnBdAsync(10, "viejo@test.com", clienteActual);

        _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(clienteActual);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<Usuario>())).ReturnsAsync(new List<string> { Roles.Cliente });
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateClienteDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _userManagerMock.Setup(u => u.FindByEmailAsync("nuevo@test.com")).ReturnsAsync((Usuario?)null);
        _repositoryMock.Setup(r => r.UpdateAsync(1, It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteActual);

        _userManagerMock.Setup(u => u.FindByIdAsync("10")).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.SetEmailAsync(usuario, "nuevo@test.com"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error cambiando email" }));

        // Act
        var result = await _service.UpdateAsync(CrearPrincipal(10), 1, CrearUpdateDto(1, email: "nuevo@test.com"), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_Devuelve_Cliente_Actualizado_Y_Limpia_Cache_Cuando_Todo_Es_Correcto()
    {
        // Arrange
        var clienteActual = CrearClienteEntidad(id: 1, idUsuario: 10);
        var usuario = await SembrarUsuarioEnBdAsync(10, "viejo@test.com", clienteActual);
        var clienteActualizado = CrearClienteEntidad(id: 1, idUsuario: 10);
        clienteActualizado.Nombre = "Juan Actualizado";

        _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(clienteActual);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<Usuario>())).ReturnsAsync(new List<string> { Roles.Cliente });
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateClienteDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _userManagerMock.Setup(u => u.FindByEmailAsync("nuevo@test.com")).ReturnsAsync((Usuario?)null);
        _repositoryMock.Setup(r => r.UpdateAsync(1, It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteActualizado);

        _userManagerMock.Setup(u => u.FindByIdAsync("10")).ReturnsAsync(usuario);
        _userManagerMock.Setup(u => u.SetEmailAsync(usuario, "nuevo@test.com")).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.SetUserNameAsync(usuario, "nuevo@test.com")).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.SetPhoneNumberAsync(usuario, "600111222")).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.UpdateAsync(usuario)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.UpdateAsync(CrearPrincipal(10), 1, CrearUpdateDto(1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Nombre.Should().Be("Juan Actualizado");
        _cacheMock.Verify(c => c.RemoveAsync("cliente:1"), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_Devuelve_ConflictError_Cuando_Lanza_DbUpdateException()
    {
        // Arrange
        var clienteActual = CrearClienteEntidad(id: 1, idUsuario: 10);
        await SembrarUsuarioEnBdAsync(10, "viejo@test.com", clienteActual);

        _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(clienteActual);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<Usuario>())).ReturnsAsync(new List<string> { Roles.Cliente });
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateClienteDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
        _repositoryMock.Setup(r => r.UpdateAsync(1, It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Unique constraint failed"));

        // Act
        var result = await _service.UpdateAsync(CrearPrincipal(10), 1, CrearUpdateDto(1), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
    }

    [Test]
    public async Task UpdateAsync_Devuelve_InternalError_Cuando_Lanza_Excepcion_Inesperada()
    {
        // Arrange
        var clienteActual = CrearClienteEntidad(id: 1, idUsuario: 10);
        await SembrarUsuarioEnBdAsync(10, "viejo@test.com", clienteActual);

        _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(clienteActual);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<Usuario>())).ReturnsAsync(new List<string> { Roles.Cliente });
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateClienteDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
        _repositoryMock.Setup(r => r.UpdateAsync(1, It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Fallo inesperado"));

        // Act
        var result = await _service.UpdateAsync(CrearPrincipal(10), 1, CrearUpdateDto(1), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<InternalError>();
    }

    #endregion

    #region DeleteAsync

    [Test]
    public async Task DeleteAsync_Devuelve_NotFound_Cuando_Cliente_No_Existe()
    {
        // Arrange
        _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

        // Act
        var result = await _service.DeleteAsync(CrearPrincipal(10), 1, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Test]
    public async Task DeleteAsync_Devuelve_Forbidden_Cuando_Cliente_Intenta_Borrar_Otro_Cliente()
    {
        // Arrange
        var clienteObjetivo = CrearClienteEntidad(id: 2, idUsuario: 20);
        var clientePropio = CrearClienteEntidad(id: 1, idUsuario: 10);
        await SembrarUsuarioEnBdAsync(10, cliente: clientePropio);

        _repositoryMock.Setup(r => r.FindByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(clienteObjetivo);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.Is<Usuario>(x => x.Id == 10)))
            .ReturnsAsync(new List<string> { Roles.Cliente });

        // Act
        var result = await _service.DeleteAsync(CrearPrincipal(10), 2, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ForbiddenError>();
    }

    [Test]
    public async Task DeleteAsync_Devuelve_ConflictError_Cuando_Cliente_Ya_Esta_Inactivo()
    {
        // Arrange
        var clienteInactivo = CrearClienteEntidad(id: 1, idUsuario: 10, isActivo: false);
        await SembrarUsuarioEnBdAsync(10);

        _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(clienteInactivo);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<Usuario>())).ReturnsAsync(new List<string> { Roles.Administrador });

        // Act
        var result = await _service.DeleteAsync(CrearPrincipal(10), 1, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
    }

    [Test]
    public async Task DeleteAsync_Devuelve_ConflictError_Cuando_Cliente_Tiene_Reservas_Vivas()
    {
        // Arrange
        var clienteActivo = CrearClienteEntidad(id: 1, idUsuario: 10, isActivo: true);
        await SembrarUsuarioEnBdAsync(10, cliente: clienteActivo);

        _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(clienteActivo);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<Usuario>())).ReturnsAsync(new List<string> { Roles.Cliente });
        _repositoryMock.Setup(r => r.TieneReservasVivasAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(CrearPrincipal(10), 1, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task DeleteAsync_Devuelve_NotFound_Cuando_Repositorio_Devuelve_False()
    {
        // Arrange
        var clienteActivo = CrearClienteEntidad(id: 1, idUsuario: 10, isActivo: true);
        await SembrarUsuarioEnBdAsync(10, cliente: clienteActivo);

        _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(clienteActivo);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<Usuario>())).ReturnsAsync(new List<string> { Roles.Cliente });
        _repositoryMock.Setup(r => r.TieneReservasVivasAsync(1)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.DeleteAsync(CrearPrincipal(10), 1, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task DeleteAsync_Devuelve_Success_Y_Elimina_De_Cache_Cuando_Todo_Es_Correcto()
    {
        // Arrange
        var clienteActivo = CrearClienteEntidad(id: 1, idUsuario: 10, isActivo: true);
        await SembrarUsuarioEnBdAsync(10, cliente: clienteActivo);

        _repositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(clienteActivo);
        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<Usuario>())).ReturnsAsync(new List<string> { Roles.Cliente });
        _repositoryMock.Setup(r => r.TieneReservasVivasAsync(1)).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(CrearPrincipal(10), 1, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _cacheMock.Verify(c => c.RemoveAsync("cliente:1"), Times.Once);
    }
    #endregion
}