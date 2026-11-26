using APIAlquilerVehiculos.Dtos.Usuarios;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Services.Usuarios;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using IdentityUserManager = Microsoft.AspNetCore.Identity.UserManager<APIAlquilerVehiculos.Models.Usuarios.Usuario>;
using IdentityUserStore = Microsoft.AspNetCore.Identity.IUserStore<APIAlquilerVehiculos.Models.Usuarios.Usuario>;
using Testcontainers.PostgreSql;

namespace APIAlquilerVehiculos.Test.Services.Test.Usuarios;

[TestFixture]
public class UsuarioServiceTests
{
    private PostgreSqlContainer _postgresContainer = null!;
    private AppDbContext _dbContext = null!;
    private Mock<IdentityUserManager> _userManagerMock = null!;
    private Mock<ICache> _cacheMock = null!;
    private Mock<ILogger<UsuarioService>> _loggerMock = null!;
    private UsuarioService _service = null!;
    

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("TestDBPostrege")
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
     
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        _dbContext = new AppDbContext(options);
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();

        // Limpiamos datos previos para aislar cada prueba
        _dbContext.UserRoles.RemoveRange(_dbContext.UserRoles);
        _dbContext.Roles.RemoveRange(_dbContext.Roles);
        _dbContext.Clientes.RemoveRange(_dbContext.Clientes);
        _dbContext.Trabajadores.RemoveRange(_dbContext.Trabajadores);
        _dbContext.Users.RemoveRange(_dbContext.Users);
        await _dbContext.SaveChangesAsync();

        _loggerMock = new Mock<ILogger<UsuarioService>>();
        _cacheMock = new Mock<ICache>();

        var storeMock = new Mock<IdentityUserStore>();
        _userManagerMock = new Mock<IdentityUserManager>(
            storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _userManagerMock.Setup(um => um.Users).Returns(() => _dbContext.Users.AsQueryable());

        _service = new UsuarioService(
            _loggerMock.Object,
            _userManagerMock.Object,
            _dbContext,
            _cacheMock.Object);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _dbContext.DisposeAsync();
    }

    #region GetPaginatedUsuariosAsync

    [Test]
    public async Task GetPaginatedUsuariosAsync_Devuelve_ListaVacia_Cuando_No_Hay_Usuarios()
    {
        // Act
        var result = await _service.GetPaginatedUsuariosAsync(1, 5, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.Count.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
        result.Value.HasPreviusPage.Should().BeFalse();
        result.Value.HasNextPage.Should().BeFalse();
    }

    [Test]
    public async Task GetPaginatedUsuariosAsync_Devuelve_PrimeraPagina_Cuando_Parametros_Son_Validos()
    {
        // Arrange
        await CrearUsuarioAsync("u1@test.com", "600111111");
        await CrearUsuarioAsync("u2@test.com", "600222222");
        await CrearUsuarioAsync("u3@test.com", "600333333");

        // Act
        var result = await _service.GetPaginatedUsuariosAsync(1, 2, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Count.Should().Be(3);
        result.Value.TotalPages.Should().Be(2);
        result.Value.HasPreviusPage.Should().BeFalse();
        result.Value.HasNextPage.Should().BeTrue();
    }

    [Test]
    public async Task GetPaginatedUsuariosAsync_Devuelve_SegundaPagina_Con_Navegacion_Correcta()
    {
        // Arrange
        await CrearUsuarioAsync("u1@test.com", null);
        await CrearUsuarioAsync("u2@test.com", null);
        await CrearUsuarioAsync("u3@test.com", null);

        // Act
        var result = await _service.GetPaginatedUsuariosAsync(2, 2, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Count.Should().Be(3);
        result.Value.TotalPages.Should().Be(2);
        result.Value.HasPreviusPage.Should().BeTrue();
        result.Value.HasNextPage.Should().BeFalse();
    }

    [Test]
    public async Task GetPaginatedUsuariosAsync_Normaliza_Valores_Cuando_PageNumber_Y_PageSize_Son_Menores_A_Uno()
    {
        // Arrange
        await CrearUsuarioAsync("u1@test.com", null);
        await CrearUsuarioAsync("u2@test.com", null);

        // Act
        var result = await _service.GetPaginatedUsuariosAsync(0, -5, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Count.Should().Be(2);
        result.Value.TotalPages.Should().Be(1);
        result.Value.HasPreviusPage.Should().BeFalse();
        result.Value.HasNextPage.Should().BeFalse();
    }

    [Test]
    public async Task GetPaginatedUsuariosAsync_Mapea_Datos_De_Cliente_Activo_E_Inactivo()
    {
        // Arrange
        var uActivo = await CrearUsuarioAsync("cliente.activo@test.com", "611111111");
        var uInactivo = await CrearUsuarioAsync("cliente.inactivo@test.com", "622222222");

        _dbContext.Clientes.AddRange(
            new Cliente { IdUsuario =uActivo.Id, Nombre = "Ana", Apellidos = "López", IsActivo = true },
            new Cliente { IdUsuario = uInactivo.Id, Nombre = "Luis", Apellidos = "Ruiz", IsActivo = false }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetPaginatedUsuariosAsync(1, 10, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dtoActivo = result.Value.Items.Single(x => x.Id == uActivo.Id);
        dtoActivo.Nombre.Should().Be("Ana");
        dtoActivo.Apellidos.Should().Be("López");
        dtoActivo.TipoUsuario.Should().Be("Cliente");
        dtoActivo.IsActivo.Should().BeTrue();

        var dtoInactivo = result.Value.Items.Single(x => x.Id == uInactivo.Id);
        dtoInactivo.Nombre.Should().Be("Luis");
        dtoInactivo.Apellidos.Should().Be("Ruiz");
        dtoInactivo.TipoUsuario.Should().Be("Cliente");
        dtoInactivo.IsActivo.Should().BeFalse();
    }

    [Test]
    public async Task GetPaginatedUsuariosAsync_Mapea_Datos_De_Trabajador_Y_Rol()
    {
        // Arrange
        var usuario = await CrearUsuarioAsync("trabajador@test.com", "633333333");
        var rol = new IdentityRole<long> { Name = "Trabajador", NormalizedName = "TRABAJADOR" };
        _dbContext.Roles.Add(rol);
        await _dbContext.SaveChangesAsync();

        _dbContext.UserRoles.Add(new IdentityUserRole<long> { UserId = usuario.Id, RoleId = rol.Id });
        _dbContext.Trabajadores.Add(new Trabajador
        {
            IdUsuario = usuario.Id,
            Nombre = "Carlos",
            Apellidos = "Martín",
            Dni = "12345678Z",
            TipoTrabajador = TipoTrabajador.Mecanico,
            IsActivo = true
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetPaginatedUsuariosAsync(1, 10, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value.Items.Single();
        dto.Nombre.Should().Be("Carlos");
        dto.Apellidos.Should().Be("Martín");
        dto.TipoUsuario.Should().Be("Trabajador");
        dto.Rol.Should().Be("Trabajador");
        dto.IsActivo.Should().BeTrue();
    }

    [Test]
    public async Task GetPaginatedUsuariosAsync_Usa_Valores_Por_Defecto_Cuando_Usuario_No_Tiene_Cliente_Ni_Trabajador()
    {
        // Arrange
        var usuario = await CrearUsuarioAsync("admin@test.com", null);

        // Act
        var result = await _service.GetPaginatedUsuariosAsync(1, 10, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value.Items.Single(x => x.Id == usuario.Id);
        dto.Nombre.Should().BeEmpty();
        dto.Apellidos.Should().BeEmpty();
        dto.TipoUsuario.Should().Be("Usuario");
        dto.Rol.Should().BeEmpty();
        dto.IsActivo.Should().BeTrue();
        dto.Telefono.Should().BeNull();
    }

    #endregion

    #region GetByIdAsync

    [Test]
    public async Task GetByIdAsync_Devuelve_NotFound_Cuando_Id_Es_Menor_O_Igual_A_Cero()
    {
        // Act
        var resultCero = await _service.GetByIdAsync(0, CancellationToken.None);
        var resultNegativo = await _service.GetByIdAsync(-3, CancellationToken.None);

        // Assert
        resultCero.IsFailure.Should().BeTrue();
        resultCero.Error.Should().BeOfType<NotFoundError>();
        resultNegativo.IsFailure.Should().BeTrue();
        resultNegativo.Error.Should().BeOfType<NotFoundError>();
        _cacheMock.Verify(c => c.GetTAsync<UsuarioDetalleDto>(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetByIdAsync_Devuelve_UsuarioDesdeCache_Cuando_Existe_En_Cache()
    {
        // Arrange
        var cachedDto = new UsuarioDetalleDto(
            Id: 50,
            Email: "cache@test.com",
            NombreYApellidos: "Elena Gómez",
            Telefono: "699999999",
            Rol: "Cliente",
            TipoUsuario: "Cliente",
            IsActivo: true,
            CreatedAt: DateTime.UtcNow.AddDays(-5),
            UpdatedAt: null);

        _cacheMock.Setup(c => c.GetTAsync<UsuarioDetalleDto>("usuario:50"))
            .ReturnsAsync(cachedDto);

        // Act
        var result = await _service.GetByIdAsync(50, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(cachedDto);
        _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<UsuarioDetalleDto>()), Times.Never);
    }

    [Test]
    public async Task GetByIdAsync_Devuelve_NotFound_Cuando_Usuario_No_Existe_En_Bd()
    {
        // Arrange
        _cacheMock.Setup(c => c.GetTAsync<UsuarioDetalleDto>("usuario:999"))
            .ReturnsAsync((UsuarioDetalleDto?)null);

        // Act
        var result = await _service.GetByIdAsync(999, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
        _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<UsuarioDetalleDto>()), Times.Never);
    }

    [Test]
    public async Task GetByIdAsync_Devuelve_Detalle_De_Cliente_Y_Guarda_En_Cache()
    {
        // Arrange
        var usuario = await CrearUsuarioAsync("cliente@test.com", "644444444");
        var rol = new IdentityRole<long> { Name = "Cliente", NormalizedName = "CLIENTE" };
        _dbContext.Roles.Add(rol);
        await _dbContext.SaveChangesAsync();

        var updated = DateTime.UtcNow;
        _dbContext.UserRoles.Add(new IdentityUserRole<long> { UserId = usuario.Id, RoleId = rol.Id });
        _dbContext.Clientes.Add(new Cliente
        {
            IdUsuario = usuario.Id,
            Nombre = "Marta",
            Apellidos = "Sanz",
            IsActivo = false,
            UpdatedAt = updated
        });
        await _dbContext.SaveChangesAsync();

        _cacheMock.Setup(c => c.GetTAsync<UsuarioDetalleDto>($"usuario:{usuario.Id}"))
            .ReturnsAsync((UsuarioDetalleDto?)null);

        // Act
        var result = await _service.GetByIdAsync(usuario.Id, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(usuario.Id);
        result.Value.Email.Should().Be("cliente@test.com");
        result.Value.NombreYApellidos.Should().Be("Marta Sanz");
        result.Value.Telefono.Should().Be("644444444");
        result.Value.Rol.Should().Be("Cliente");
        result.Value.TipoUsuario.Should().Be("Cliente");
        result.Value.IsActivo.Should().BeFalse();
        result.Value.UpdatedAt.Should().NotBeNull();
        _cacheMock.Verify(c => c.SetAsync($"usuario:{usuario.Id}", It.IsAny<UsuarioDetalleDto>()), Times.Once);
    }

    [Test]
    public async Task GetByIdAsync_Devuelve_Detalle_De_Trabajador_Cuando_Es_Trabajador()
    {
        // Arrange
        var usuario = await CrearUsuarioAsync("empleado@test.com", "655555555");
        _dbContext.Trabajadores.Add(new Trabajador
        {
            IdUsuario = usuario.Id,
            Nombre = "Diego",
            Apellidos = "Pérez",
            Dni = "12345678Z",
            TipoTrabajador = TipoTrabajador.Mecanico,
            IsActivo = true
        });
        await _dbContext.SaveChangesAsync();

        _cacheMock.Setup(c => c.GetTAsync<UsuarioDetalleDto>($"usuario:{usuario.Id}"))
            .ReturnsAsync((UsuarioDetalleDto?)null);

        // Act
        var result = await _service.GetByIdAsync(usuario.Id, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.NombreYApellidos.Should().Be("Diego Pérez");
        result.Value.TipoUsuario.Should().Be("Trabajador");
        result.Value.IsActivo.Should().BeTrue();
    }

    [Test]
    public async Task GetByIdAsync_Devuelve_Detalle_Admin_Cuando_No_Tiene_Cliente_Ni_Trabajador()
    {
        // Arrange
        var usuario = await CrearUsuarioAsync("root@test.com", null);

        _cacheMock.Setup(c => c.GetTAsync<UsuarioDetalleDto>($"usuario:{usuario.Id}"))
            .ReturnsAsync((UsuarioDetalleDto?)null);

        // Act
        var result = await _service.GetByIdAsync(usuario.Id, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.NombreYApellidos.Should().Be("Admin");
        result.Value.TipoUsuario.Should().Be("Admin");
        result.Value.IsActivo.Should().BeTrue();
        result.Value.UpdatedAt.Should().BeNull();
        result.Value.Rol.Should().BeEmpty();
    }

    #endregion

    private async Task<Usuario> CrearUsuarioAsync(string email, string? telefono)
    {
        var usuario = new Usuario
        {
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PhoneNumber = telefono,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(usuario);
        await _dbContext.SaveChangesAsync();
        return usuario;
    }
}