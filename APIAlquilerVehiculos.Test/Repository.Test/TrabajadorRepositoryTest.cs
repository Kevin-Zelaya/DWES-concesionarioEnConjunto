using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Repositories.Trabajadores;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace APIAlquilerVehiculos.Test.Repository.Test;

[TestFixture]
public class TrabajadorRepositoryTest
{
    private PostgreSqlContainer _postgresContainer = null!;
    private AppDbContext _db = null!;
    private TrabajadorRepository _repository = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("test_alquiler_vehiculos_trabajadores")
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

        _repository = new TrabajadorRepository(_db);

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
            UserName = "trabajador1",
            Email = "t1@test.com",
            NormalizedEmail = "T1@TEST.COM",
            NormalizedUserName = "TRABAJADOR1"
        };
        var user2 = new Usuario
        {
            Id = 2,
            UserName = "trabajador2",
            Email = "t2@test.com",
            NormalizedEmail = "T2@TEST.COM",
            NormalizedUserName = "TRABAJADOR2"
        };
        var user3 = new Usuario
        {
            Id = 3,
            UserName = "trabajador3",
            Email = "t3@test.com",
            NormalizedEmail = "T3@TEST.COM",
            NormalizedUserName = "TRABAJADOR3"
        };
        var user4 = new Usuario
        {
            Id = 4,
            UserName = "cliente1",
            Email = "c1@test.com",
            NormalizedEmail = "C1@TEST.COM",
            NormalizedUserName = "CLIENTE1"
        };

        await _db.Users.AddRangeAsync(user1, user2, user3, user4);

        var t1 = new Trabajador
        {
            Id = 1,
            IdUsuario = 1,
            Nombre = "Zacarías",
            Apellidos = "Alvarez",
            Dni = "11111111A",
            FechaIngreso = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TipoTrabajador = TipoTrabajador.Operaciones,
            IsActivo = true
        };

        var t2 = new Trabajador
        {
            Id = 2,
            IdUsuario = 2,
            Nombre = "Bernardo",
            Apellidos = "Benitez",
            Dni = "22222222B",
            FechaIngreso = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TipoTrabajador = TipoTrabajador.Mecanico,
            IsActivo = true
        };

        var t3 = new Trabajador
        {
            Id = 3,
            IdUsuario = 3,
            Nombre = "Carlos",
            Apellidos = "Inactivo",
            Dni = "33333333C",
            FechaIngreso = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TipoTrabajador = TipoTrabajador.Operaciones,
            IsActivo = false,
            DeletedAt = DateTime.UtcNow
        };

        await _db.Trabajadores.AddRangeAsync(t1, t2, t3);

        var cliente1 = new Cliente
        {
            Id = 1,
            IdUsuario = 4,
            Nombre = "Cliente",
            Apellidos = "Prueba",
            Dni = "55555555E",
            FechaNacimiento = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsActivo = true
        };
        await _db.Clientes.AddAsync(cliente1);

        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();
    }

    #region GetAllAsync

    [Test]
    public async Task GetAllAsync_DevuelveSoloTrabajadoresActivosOrdenadosPorApellidos()
    {
        var result = await _repository.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.IsActivo);
        result.Select(t => t.Apellidos).Should().BeInAscendingOrder();
        result[0].Usuario.Should().NotBeNull();
    }

    #endregion

    #region GetAllPagedAsync

    [Test]
    public async Task GetAllPagedAsync_SinFiltros_DevuelveActivosPaginadosYOrdenadosPorApellidosYNombre()
    {
        var (items, total) = await _repository.GetAllPagedAsync(page: 1, pageSize: 10);

        total.Should().Be(2);
        items.Should().HaveCount(2);
        items[0].Apellidos.Should().Be("Alvarez");
    }

    [Test]
    public async Task GetAllPagedAsync_IncluyendoInactivos_DevuelveTodos()
    {
        var (items, total) = await _repository.GetAllPagedAsync(page: 1, pageSize: 10, incluirInactivos: true);

        total.Should().Be(3);
        items.Should().HaveCount(3);
    }

    [Test]
    public async Task GetAllPagedAsync_FiltrandoPorTipo_DevuelveCoincidentes()
    {
        var (items, total) = await _repository.GetAllPagedAsync(page: 1, pageSize: 10, tipo: TipoTrabajador.Mecanico);

        total.Should().Be(1);
        items.Should().ContainSingle();
        items[0].TipoTrabajador.Should().Be(TipoTrabajador.Mecanico);
    }

    #endregion

    #region GetByIdAsync

    [Test]
    public async Task GetByIdAsync_ConIdExistenteActivoOInactivo_DevuelveTrabajador()
    {
        var activo = await _repository.GetByIdAsync(1);
        activo.Should().NotBeNull();
        activo!.Nombre.Should().Be("Zacarías");

        var inactivo = await _repository.GetByIdAsync(3);
        inactivo.Should().NotBeNull();
        inactivo!.IsActivo.Should().BeFalse();
    }

    [Test]
    public async Task GetByIdAsync_ConIdInexistente_DevuelveNull()
    {
        var result = await _repository.GetByIdAsync(999);
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdUsuarioAsync

    [Test]
    public async Task GetByIdUsuarioAsync_ConIdUsuarioExistente_DevuelveTrabajador()
    {
        var result = await _repository.GetByIdUsuarioAsync(1);

        result.Should().NotBeNull();
        result!.IdUsuario.Should().Be(1);
    }

    [Test]
    public async Task GetByIdUsuarioAsync_ConIdUsuarioInexistente_DevuelveNull()
    {
        var result = await _repository.GetByIdUsuarioAsync(999);
        result.Should().BeNull();
    }

    #endregion

    #region CreateAsync

    [Test]
    public async Task CreateAsync_InsertaYDevuelveTrabajador()
    {
        var nuevoUsuario = new Usuario { Id = 8, UserName = "nuevo", Email = "nuevo@test.com" };
        await _db.Users.AddAsync(nuevoUsuario);
        await _db.SaveChangesAsync();

        var nuevoTrabajador = new Trabajador
        {
            Id = 4,
            IdUsuario = 8,
            Nombre = "Lucía",
            Apellidos = "Gómez",
            Dni = "44444444D",
            FechaIngreso = DateTime.UtcNow,
            TipoTrabajador = TipoTrabajador.Operaciones,
            IsActivo = true
        };

        var creado = await _repository.CreateAsync(nuevoTrabajador);

        creado.Should().NotBeNull();
        creado.Id.Should().Be(4);

        var enDb = await _db.Trabajadores.FindAsync(4L);
        enDb.Should().NotBeNull();
    }

    #endregion

    #region UpdateAsync

    [Test]
    public async Task UpdateAsync_ConTrabajadorExistente_ActualizaYDevuelveInstancia()
    {
        var modificado = new Trabajador
        {
            Id = 1,
            Nombre = "Zacarías Modificado",
            Apellidos = "Álvarez Nuevo",
            Dni = "11111111A",
            FechaIngreso = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TipoTrabajador = TipoTrabajador.Mecanico,
            IsActivo = true
        };

        var resultado = await _repository.UpdateAsync(modificado);

        resultado.Should().NotBeNull();
        resultado!.Nombre.Should().Be("Zacarías Modificado");
        resultado.TipoTrabajador.Should().Be(TipoTrabajador.Mecanico);
        resultado.UpdatedAt.Should().NotBeNull();
    }

    [Test]
    public async Task UpdateAsync_ConTrabajadorInexistente_DevuelveNull()
    {
        var noExiste = new Trabajador
        {
            Id = 999,
            Nombre = "Fantasmas",
            Apellidos = "Fantasmas",
            Dni = "99999999F",
            TipoTrabajador = TipoTrabajador.Operaciones,
            IsActivo = true
        };

        var resultado = await _repository.UpdateAsync(noExiste);

        resultado.Should().BeNull();
    }

    #endregion

    #region DeleteAsync

    [Test]
    public async Task DeleteAsync_ConIdExistente_RealizaBajaLogicaDevuelveTrue()
    {
        var exito = await _repository.DeleteAsync(1);

        exito.Should().BeTrue();

        _db.ChangeTracker.Clear();
        var enDb = await _db.Trabajadores.IgnoreQueryFilters().FirstAsync(t => t.Id == 1);
        enDb.IsActivo.Should().BeFalse();
        enDb.DeletedAt.Should().NotBeNull();
    }

    [Test]
    public async Task DeleteAsync_ConIdInexistente_DevuelveFalse()
    {
        var exito = await _repository.DeleteAsync(999);
        exito.Should().BeFalse();
    }

    #endregion

    #region ExistsByDniAsync

    [Test]
    public async Task ExistsByDniAsync_DevuelveTrueSiExisteYFalseSiNo()
    {
        var existe = await _repository.ExistsByDniAsync("11111111A");
        var noExiste = await _repository.ExistsByDniAsync("00000000X");

        existe.Should().BeTrue();
        noExiste.Should().BeFalse();
    }

    #endregion

    #region EsResponsableAsync

    [Test]
    public async Task EsResponsableAsync_DevuelveTrueSiEsResponsableDeAlgunaOficina()
    {
        var oficina = new Oficina
        {
            Id = 1,
            Name = "Oficina Central",
            IdResponsable = 1,
            Telefono = "600000000",
            Horario = "09:00-18:00",
            Direccion = new Direccion
            {
                Calle = "Calle Falsa 123",
                CodigoPostal = "28001",
                Ciudad = "Madrid"
            },
            Localizacion = new Localizacion
            {
                Latitud = 40.4168m,
                Longitud = -3.7038m
            },
            Activo = true
        };
        await _db.Oficinas.AddAsync(oficina);
        await _db.SaveChangesAsync();

        var esResponsable = await _repository.EsResponsableAsync(1);
        var noEsResponsable = await _repository.EsResponsableAsync(2);

        esResponsable.Should().BeTrue();
        noEsResponsable.Should().BeFalse();
    }

    #endregion

    #region TieneReservasVivasAsync

    [TestCase(Estado.Pendiente, true)]
    [TestCase(Estado.Confirmada, true)]
    [TestCase(Estado.EnCurso, true)]
    [TestCase(Estado.Finalizada, false)]
    [TestCase(Estado.Cancelada, false)]
    public async Task TieneReservasVivasAsync_EvaluaCorrectamenteSegunEstado(Estado estado, bool esperado)
    {
        var marca = new Marca
        {
            Nombre = "Marca Test" // ajusta las propiedades requeridas de Marca
        };
        _db.Marcas.Add(marca);
        await _db.SaveChangesAsync();
        
        var vehiculo = new Vehiculo
        {
            Id = "VEH-01",
            Matricula = "9999XYZ",
            MarcaId = marca.Id,
            Modelo = "Focus",
            Acriss = "CCMR",
            categoria = CategoriaEnum.Familiar,
            Concesionario_id = 1,
            precioPorDia = 40,
            Color = "Blanco",
            Combustible = CombustibleEnum.Gasolina,
            Maletero = 500,
            NumeroPlazas = 5,
            Autonomia = 800,
            Imagenes = 0,
            Estado = EstadoEnum.Disponible,
            UltimaRevision = DateTime.UtcNow,
            Activo = true
        };
        await _db.Vehiculos.AddAsync(vehiculo);

        var reserva = new Reserva
        {
            Id = 50,
            ClienteId = 1,
            TrabajadorId = 1,
            VehiculoId = vehiculo.Id,
            Estado = estado,
            Categoria = CategoriaEnum.Familiar,
            FechaCreacion = DateTime.UtcNow,
            FechaRecogida = DateTime.UtcNow.AddDays(1),
            FechaDevolucion = DateTime.UtcNow.AddDays(3),
            PrecioTotal = 100m
        };
        await _db.Reservas.AddAsync(reserva);
        await _db.SaveChangesAsync();

        var resultado = await _repository.TieneReservasVivasAsync(1);

        resultado.Should().Be(esperado);
    }

    #endregion
}