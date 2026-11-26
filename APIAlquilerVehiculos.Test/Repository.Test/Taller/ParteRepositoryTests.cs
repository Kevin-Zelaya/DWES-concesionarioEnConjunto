using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Repositories.Taller;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace APIAlquilerVehiculos.Test.Repository.Test.Taller;

[SetUpFixture]
public class TallerPostgresFixture
{
    public static PostgreSqlContainer Container { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        Container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();

        await Container.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(Container.GetConnectionString())
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        if (Container is not null)
        {
            await Container.DisposeAsync();
        }
    }
}

public abstract class ParteRepositoryTests
{
    protected AppDbContext _context = null!;
    protected ParteRepository _repository = null!;
    protected Parte _parteSemilla = null!;
    protected string _vehiculoId = null!;
    protected long _trabajadorId;

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(TallerPostgresFixture.Container.GetConnectionString())
            .Options;

        _context = new AppDbContext(options);

        var tableNames = _context.Model.GetEntityTypes()
            .Select(t => t.GetTableName())
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .Select(name => $"\"{name}\"");

        var sql = $"TRUNCATE TABLE {string.Join(", ", tableNames)} RESTART IDENTITY CASCADE;";
        await _context.Database.ExecuteSqlRawAsync(sql);

        _repository = new ParteRepository(_context);

        // 1. Crear Usuario y Trabajador
        var usuario = new Usuario
        {
            UserName = "carlos@empresa.es",
            Email = "carlos@empresa.es",
            CreatedAt = DateTime.UtcNow
        };
        await _context.Users.AddAsync(usuario);
        await _context.SaveChangesAsync();

        var trabajador = new Trabajador
        {
            IdUsuario = usuario.Id,
            Nombre = "Carlos",
            Apellidos = "Gómez",
            Dni = "12345678Z",
            IsActivo = true,
            TipoTrabajador = TipoTrabajador.Mecanico,
            CreatedAt = DateTime.UtcNow
        };

        // 2. Crear Marca obligatoria por la FK de VehiculoYRelacionadosConfig
        var marca = new Marca
        {
            Nombre = "Toyota",
            Wb = "https://www.toyota.es",
            Activo = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Trabajadores.AddAsync(trabajador);
        await _context.Marcas.AddAsync(marca);
        await _context.SaveChangesAsync();
        _trabajadorId = trabajador.Id;

        // 3. Crear Vehículo (el Id de 11 chars lo genera YoutubeIdValueGenerator)
        var vehiculo = new Vehiculo
        {
            Matricula = "1234BCD",
            MarcaId = marca.Id,
            Modelo = "Corolla",
            Acriss = "CDMR",
            categoria = CategoriaEnum.Hatchback,
            Concesionario_id = 1,
            precioPorDia = 45.00m,
            Color = "Blanco",
            Combustible = CombustibleEnum.Gasolina,
            Maletero = 380,
            NumeroPlazas = 5,
            Autonomia = 650,
            Imagenes = 1,
            Estado = EstadoEnum.Disponible,
            UltimaRevision = DateTime.UtcNow,
            Activo = true
        };

        await _context.Vehiculos.AddAsync(vehiculo);
        await _context.SaveChangesAsync();
        _vehiculoId = vehiculo.Id;

        // 4. Crear Parte semilla
        _parteSemilla = await _repository.CreateAsync(new Parte
        {
            FechaApertura = DateTime.UtcNow,
            QuienLoTramitaId = _trabajadorId,
            Descripcion = "Revisión inicial",
            VehiculoId = _vehiculoId,
            Estado = EstadoParte.Abierto
        });
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.DisposeAsync();
    }

    [TestFixture]
    public class CasosValidos : ParteRepositoryTests
    {
        [Test]
        public async Task CrudBasico_CreateGetAllGetByIdUpdateYDelete_FuncionanCorrectamente()
        {
            // Arrange
            var nuevoParte = new Parte
            {
                FechaApertura = DateTime.UtcNow,
                QuienLoTramitaId = _trabajadorId,
                Descripcion = "Cambio de aceite",
                VehiculoId = _vehiculoId,
                Estado = EstadoParte.Abierto
            };

            // Act (Create & GetAll)
            var creado = await _repository.CreateAsync(nuevoParte);
            var todos = await _repository.GetAllAsync();

            // Assert (Create & GetAll)
            creado.Id.Should().BeGreaterThan(0);
            todos.Should().HaveCount(2);

            // Act (GetById & Update)
            var obtenido = await _repository.GetByIdAsync(creado.Id);
            obtenido!.Estado = EstadoParte.Cerrado;
            var actualizado = await _repository.UpdateAsync(obtenido);

            // Assert (GetById & Update)
            actualizado.Should().NotBeNull();
            actualizado!.Estado.Should().Be(EstadoParte.Cerrado);

            // Act (Delete)
            var eliminado = await _repository.DeleteAsync(creado.Id);
            var trasEliminar = await _repository.GetAllAsync();

            // Assert (Delete)
            eliminado.Should().BeTrue();
            trasEliminar.Should().HaveCount(1);
        }

        [TestCase(EstadoParte.Abierto, true)]
        [TestCase(EstadoParte.Cerrado, false)]
        public async Task TieneParteAbiertoAsync_SegunEstadoDelParte_DevuelveResultadoEsperado(
            EstadoParte estado, bool esperado)
        {
            // Arrange
            _parteSemilla.Estado = estado;
            await _repository.UpdateAsync(_parteSemilla);

            // Act
            var resultado = await _repository.TieneParteAbiertoAsync(_vehiculoId);

            // Assert
            resultado.Should().Be(esperado);
        }

        [Test]
        public async Task ConsultasApoyo_TrabajadorVehiculoReservasYUpdateVehiculo_DevuelvenDatosSemilla()
        {
            // Arrange
            var vehiculo = await _repository.GetVehiculoByIdAsync(_vehiculoId);

            // Act
            var trabajador = await _repository.GetTrabajadorByIdAsync(_trabajadorId);
            var reservas = await _repository.GetReservasVivasByVehiculoIdAsync(_vehiculoId);
            vehiculo!.Estado = EstadoEnum.EnMantenimiento;
            await _repository.UpdateVehiculoAsync(vehiculo);

            // Assert
            trabajador.Should().NotBeNull();
            trabajador!.Id.Should().Be(_trabajadorId);
            vehiculo.Should().NotBeNull();
            vehiculo.Matricula.Should().Be("1234BCD");
            vehiculo.Estado.Should().Be(EstadoEnum.EnMantenimiento);
            reservas.Should().BeEmpty();
        }
    }

    [TestFixture]
    public class CasosInvalidos : ParteRepositoryTests
    {
        [Test]
        public async Task GetByIdUpdateYDelete_CuandoIdNoExiste_DevuelvenNullOFalse()
        {
            // Arrange
            var parteInexistente = new Parte { Id = 99999, Descripcion = "No existe" };

            // Act
            var obtenido = await _repository.GetByIdAsync(99999);
            var actualizado = await _repository.UpdateAsync(parteInexistente);
            var eliminado = await _repository.DeleteAsync(99999);

            // Assert
            obtenido.Should().BeNull();
            actualizado.Should().BeNull();
            eliminado.Should().BeFalse();
        }

        [Test]
        public async Task ConsultasApoyo_CuandoVehiculoOTrabajadorNoExisten_DevuelvenNullOFalse()
        {
            // Arrange
            const string vehiculoInexistente = "inexistente";
            const long trabajadorInexistente = 99999;

            // Act
            var tieneParte = await _repository.TieneParteAbiertoAsync(vehiculoInexistente);
            var trabajador = await _repository.GetTrabajadorByIdAsync(trabajadorInexistente);
            var vehiculo = await _repository.GetVehiculoByIdAsync(vehiculoInexistente);

            // Assert
            tieneParte.Should().BeFalse();
            trabajador.Should().BeNull();
            vehiculo.Should().BeNull();
        }
    }
}