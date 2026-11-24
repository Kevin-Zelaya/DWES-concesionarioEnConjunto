using APIAlquilerVehiculos.Dtos.Cientes;
using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Repositories.Clientes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace APIAlquilerVehiculos.Test.Repository.Test;

[TestFixture]
public class ClientesRepositoryTest
{
    private PostgreSqlContainer _postgresContainer = null!;
    private AppDbContext _db = null!;
    private ClienteRepository _repository = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("test_alquiler_vehiculos")
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

        _db = new AppDbContext(options);
        await _db.Database.EnsureDeletedAsync();
        await _db.Database.EnsureCreatedAsync();

        _repository = new ClienteRepository(_db);

        await SeedDataAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _db.DisposeAsync();
    }

    private async Task SeedDataAsync()
    {
        var user1 = new Usuario
        {
            Id = 1,
            UserName = "juan.perez",
            Email = "juan.perez@test.com",
            NormalizedEmail = "JUAN.PEREZ@TEST.COM",
            NormalizedUserName = "JUAN.PEREZ"
        };
        var user2 = new Usuario
        {
            Id = 2,
            UserName = "ana.garcia",
            Email = "ana.garcia@test.com",
            NormalizedEmail = "ANA.GARCIA@TEST.COM",
            NormalizedUserName = "ANA.GARCIA"
        };
        var user3 = new Usuario
        {
            Id = 3,
            UserName = "carlos.inactivo",
            Email = "carlos@test.com",
            NormalizedEmail = "CARLOS@TEST.COM",
            NormalizedUserName = "CARLOS.INACTIVO"
        };

        await _db.Users.AddRangeAsync(user1, user2, user3);

        var cliente1 = new Cliente
        {
            Id = 1,
            IdUsuario = 1,
            Nombre = "Juan",
            Apellidos = "Pérez López",
            Dni = "12345678A",
            NumeroPermisoConducir = "PERM-001",
            FechaNacimiento = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            IsActivo = true
        };

        var cliente2 = new Cliente
        {
            Id = 2,
            IdUsuario = 2,
            Nombre = "Ana",
            Apellidos = "García Ruiz",
            Dni = "87654321B",
            NumeroPermisoConducir = "PERM-002",
            FechaNacimiento = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            IsActivo = true
        };

        var cliente3 = new Cliente
        {
            Id = 3,
            IdUsuario = 3,
            Nombre = "Carlos",
            Apellidos = "Sánchez",
            Dni = "11223344C",
            NumeroPermisoConducir = "PERM-003",
            FechaNacimiento = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            IsActivo = false,
            DeletedAt = DateTime.UtcNow.AddDays(-2)
        };

        await _db.Clientes.AddRangeAsync(cliente1, cliente2, cliente3);
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();
    }

    #region FindAllPagedAsync

    [Test]
    public async Task FindAllPagedAsync_Devuelve_Clientes_Activos_PorDefecto()
    {
        // Act
        var (items, totalCount) = await _repository.FindAllPagedAsync(1, 10);

        // Assert
        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
        items.Should().OnlyContain(c => c.IsActivo);
        items.Should().OnlyContain(c => c.Usuario != null);
    }

    [Test]
    public async Task FindAllPagedAsync_Devuelve_Todos_Clientes_Incluyendo_Inactivos()
    {
        // Act
        var (items, totalCount) = await _repository.FindAllPagedAsync(1, 10, incluirInactivos: true);

        // Assert
        totalCount.Should().Be(3);
        items.Should().HaveCount(3);
        items.Should().Contain(c => !c.IsActivo && c.Nombre == "Carlos");
    }

    [Test]
    public async Task FindAllPagedAsync_Devuelve_Clientes_Ordenados_Por_Apellidos_Y_Nombre()
    {
        // Act (En SeedData: Ana García Ruiz, Juan Pérez López)
        var (items, totalCount) = await _repository.FindAllPagedAsync(1, 10, incluirInactivos: false);

        // Assert
        totalCount.Should().Be(2);
        items[0].Apellidos.Should().Be("García Ruiz");
        items[1].Apellidos.Should().Be("Pérez López");
    }

    [Test]
    public async Task FindAllPagedAsync_Devuelve_Primera_Pagina_Con_Tamano_Pagina()
    {
        // Act
        var (items, totalCount) = await _repository.FindAllPagedAsync(1, 1, incluirInactivos: false);

        // Assert
        totalCount.Should().Be(2);
        items.Should().ContainSingle();
        items[0].Apellidos.Should().Be("García Ruiz");
    }

    [Test]
    public async Task FindAllPagedAsync_Devuelve_Segunda_Pagina_Correctamente()
    {
        // Act
        var (items, totalCount) = await _repository.FindAllPagedAsync(2, 1, incluirInactivos: false);

        // Assert
        totalCount.Should().Be(2);
        items.Should().ContainSingle();
        items[0].Apellidos.Should().Be("Pérez López");
    }

    [Test]
    public async Task FindAllPagedAsync_Devuelve_Lista_Vacia_Cuando_Pagina_Supera_Resultados()
    {
        // Act
        var (items, totalCount) = await _repository.FindAllPagedAsync(10, 10, incluirInactivos: false);

        // Assert
        totalCount.Should().Be(2);
        items.Should().BeEmpty();
    }

    #endregion

    #region FindByIdAsync

    [Test]
    public async Task FindByIdAsync_Devuelve_Cliente_ConUsuario_Cuando_ExisteYActivo()
    {
        var result = await _repository.FindByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Nombre.Should().Be("Juan");
        result.Usuario.Should().NotBeNull();
    }

    [Test]
    public async Task FindByIdAsync_Devuelve_Cliente_Si_Inactivo()
    {
        var result = await _repository.FindByIdAsync(3);

        result.Should().NotBeNull();
        result!.Id.Should().Be(3);
        result.IsActivo.Should().BeFalse();
    }

    [Test]
    public async Task FindByIdAsync_Devuelve_Null_Cuando_Id_NoExiste()
    {
        var result = await _repository.FindByIdAsync(999);

        result.Should().BeNull();
    }

    #endregion

    #region FindByUserId

    [Test]
    public async Task FindByUserId_Devuelve_Cliente_Cuando_Existe_Y_Activo()
    {
        var result = await _repository.FindByUserId(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.IdUsuario.Should().Be(1);
    }

    [Test]
    public async Task FindByUserId_Devuelve_Null_Si_Cliente_Inactivo()
    {
        var result = await _repository.FindByUserId(3);

        result.Should().BeNull();
    }

    [Test]
    public async Task FindByUserId_Devuelve_Null_Cuando_NoExiste_ClienteParaEseUsuario()
    {
        var result = await _repository.FindByUserId(999);

        result.Should().BeNull();
    }

    #endregion

    #region UpdateAsync

    [Test]
    public async Task UpdateAsync_Devuelve_Cliente_Actualizado_Cuand_Existe()
    {
        var datosActualizados = new Cliente
        {
            Nombre = "Juan Actualizado",
            Apellidos = "Pérez Nuevo",
            FechaNacimiento = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            Dni = "99999999Z",
            Nie = "X1234567L",
            Pasaporte = "PAS98765",
            NumeroPermisoConducir = "PERM-999",
            IsActivo = true
        };

        var result = await _repository.UpdateAsync(1, datosActualizados);

        result.Should().NotBeNull();
        result!.Nombre.Should().Be("Juan Actualizado");
        result.Apellidos.Should().Be("Pérez Nuevo");
        result.FechaNacimiento.Should().Be(new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        result.Dni.Should().Be("99999999Z");
        result.Nie.Should().Be("X1234567L");
        result.Pasaporte.Should().Be("PAS98765");
        result.NumeroPermisoConducir.Should().Be("PERM-999");
        result.UpdatedAt.Should().NotBeNull();
    }

    [Test]
    public async Task UpdateAsync_Devuelve_Cliente_Actualizado_Si_Esta_Inactivo()
    {
        var datosActualizados = new Cliente
        {
            Nombre = "Carlos Actualizado",
            Apellidos = "Sánchez Nuevo",
            FechaNacimiento = new DateTime(1998, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            Dni = "11223344C",
            NumeroPermisoConducir = "PERM-003",
            IsActivo = false
        };

        var result = await _repository.UpdateAsync(3, datosActualizados);

        result.Should().NotBeNull();
        result!.Nombre.Should().Be("Carlos Actualizado");
    }

    [Test]
    public async Task UpdateAsync_Devuelve_Null_Cuando_Cliente_NoExiste()
    {
        var clienteInexistente = new Cliente
        {
            Nombre = "Juan",
            Apellidos = "Canavaro",
            IsActivo = true
        };

        var result = await _repository.UpdateAsync(999, clienteInexistente);

        result.Should().BeNull();
    }

    #endregion
    
    #region UpdatePartialAsync

    [Test]
    public async Task UpdatePartialAsync_Devuelve_Cliente_Reactivado_Cuando_Cliente_Estaba_Inactivo()
    {
        // Arrange
        const long clienteId = 3;

        // Act
        var result = await _repository.UpdatePartialAsync(clienteId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(clienteId);
        result.IsActivo.Should().BeTrue();
        result.DeletedAt.Should().BeNull();
        result.UpdatedAt.Should().NotBeNull();

        _db.ChangeTracker.Clear();
        var clienteEnBd = await _db.Clientes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == clienteId);

        clienteEnBd.Should().NotBeNull();
        clienteEnBd!.IsActivo.Should().BeTrue();
        clienteEnBd.DeletedAt.Should().BeNull();
        clienteEnBd.UpdatedAt.Should().NotBeNull();
    }

    [Test]
    public async Task UpdatePartialAsync_Devuelve_Cliente_Actualizado_Cuando_Cliente_Ya_Estaba_Activo()
    {
        // Arrange
        const long clienteId = 1;

        // Act
        var result = await _repository.UpdatePartialAsync(clienteId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(clienteId);
        result.IsActivo.Should().BeTrue();
        result.DeletedAt.Should().BeNull();
        result.UpdatedAt.Should().NotBeNull();

        _db.ChangeTracker.Clear();
        var clienteEnBd = await _db.Clientes
            .FirstOrDefaultAsync(c => c.Id == clienteId);

        clienteEnBd.Should().NotBeNull();
        clienteEnBd!.IsActivo.Should().BeTrue();
        clienteEnBd.DeletedAt.Should().BeNull();
    }

    [Test]
    public async Task UpdatePartialAsync_Devuelve_Null_Cuando_Cliente_No_Existe()
    {
        // Arrange
        const long idInexistente = 999999;

        // Act
        var result = await _repository.UpdatePartialAsync(idInexistente);

        // Assert
        result.Should().BeNull();
    }
    #endregion

    #region DeleteAsync

    [Test]
    public async Task DeleteAsync_Devuelve_True_Y_Marca_ComoInactivo_Cuando_Cliente_Existe()
    {
        var deleted = await _repository.DeleteAsync(1);

        deleted.Should().BeTrue();

        _db.ChangeTracker.Clear();
        var clienteEnBd = await _db.Clientes.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == 1);
        clienteEnBd.Should().NotBeNull();
        clienteEnBd!.IsActivo.Should().BeFalse();
        clienteEnBd.DeletedAt.Should().NotBeNull();
    }

    [Test]
    public async Task DeleteAsync_Devuelve_False_Cuando_Cliente_Esta_Inactivo()
    {
        var deleted = await _repository.DeleteAsync(3);

        deleted.Should().BeFalse();
    }

    [Test]
    public async Task DeleteAsync_Devuelve_False_Cuando_Cliente_NoExiste()
    {
        var deleted = await _repository.DeleteAsync(999);

        deleted.Should().BeFalse();
    }

    #endregion

    #region ExistsClientesWithIdUserAsync

    [Test]
    public async Task ExistsClientesWithIdUserAsync_Devuelve_True_Cuando_Existe_Cliente_Activo()
    {
        var exists = await _repository.ExistsClientesWithIdUserAsync(1);

        exists.Should().BeTrue();
    }

    [Test]
    public async Task ExistsClientesWithIdUserAsync_Devuelve_False_Cuando_Cliente_Inactivo()
    {
        var exists = await _repository.ExistsClientesWithIdUserAsync(3);

        exists.Should().BeFalse();
    }

    [Test]
    public async Task ExistsClientesWithIdUserAsync_Devuelve_False_Cuando_NoExiste_ClientePara_Usuario()
    {
        var exists = await _repository.ExistsClientesWithIdUserAsync(999);

        exists.Should().BeFalse();
    }

    #endregion

    #region TieneReservasVivasAsync

    [TestCase(Estado.Pendiente, true)]
    [TestCase(Estado.Confirmada, true)]
    [TestCase(Estado.EnCurso, true)]
    [TestCase(Estado.Finalizada, false)]
    [TestCase(Estado.Caducada, false)]
    [TestCase(Estado.Cancelada, false)]
    [TestCase(Estado.NoPresentado, false)]
    public async Task TieneReservasVivasAsync_Devuelve_True_Si_Reserva_Esta_Pendiente_Confirmada_EnCurso(Estado estado, bool esperado)
    {
        var marca = new Marca
        {
            Nombre = "Marca Test" // ajusta las propiedades requeridas de Marca
        };
        _db.Marcas.Add(marca);
        await _db.SaveChangesAsync();
        
        var vehiculo = new Vehiculo
        {
            Id = "deUKZ4KCd9J",
            MarcaId = marca.Id,
            Modelo = "Corolla",
            Matricula = "1234ABC"
        };
        await _db.Vehiculos.AddAsync(vehiculo);

        var reserva = new Reserva
        {
            Id = 100,
            ClienteId = 1,
            VehiculoId = vehiculo.Id,
            Categoria = CategoriaEnum.Familiar,
            Estado = estado,
            FechaCreacion = DateTime.UtcNow,
            FechaRecogida = DateTime.SpecifyKind(DateTime.Now.AddDays(1), DateTimeKind.Unspecified),
            FechaDevolucion = DateTime.SpecifyKind(DateTime.Now.AddDays(3), DateTimeKind.Unspecified),
            PrecioTotal = 150m
        };

        await _db.Reservas.AddAsync(reserva);
        await _db.SaveChangesAsync();

        var tieneVivas = await _repository.TieneReservasVivasAsync(1);

        tieneVivas.Should().Be(esperado);
    }

    [Test]
    public async Task TieneReservasVivasAsync_Devuelve_False_CuandoCliente_NoTien_eNingunaReserva()
    {
        var tieneVivas = await _repository.TieneReservasVivasAsync(2);

        tieneVivas.Should().BeFalse();
    }

    #endregion
}