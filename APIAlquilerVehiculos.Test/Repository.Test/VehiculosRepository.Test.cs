using APIAlquilerVehiculos.Entities;
using APIAlquilerVehiculos.Entities.FluenApi;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Repositories.Vehiculos;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace APIAlquilerVehiculos.Test.Repository.Test;

[TestFixture]
[Parallelizable]
public class VehiculosRepository_Test
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("Vehiculo_test")
        .WithUsername("VehiculoTest")
        .WithPassword("Test")
        .Build();

    private AppDbContext _context = null!;
    private VehiculoRepository _repository = null!;

    [OneTimeSetUp]
    public async Task StartPostgresAsync()
    {
        await _postgres.StartAsync();
    }

    [SetUp]
    public async Task SetUpAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        var marca = new Marca { Nombre = "Test", Wb = "https://example.test", Activo = true };
        var modelo = "Golf";
        _context.AddRange(marca, modelo);
        await _context.SaveChangesAsync();

        _repository = new VehiculoRepository(_context);
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        await _context.DisposeAsync();
    }

    [OneTimeTearDown]
    public async Task StopPostgresAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Test]
    public async Task GetAllAsync_DevuelveSoloVehiculosActivos()
    {
        await AddVehiculosAsync(
            CrearVehiculo("vehiculo01", "AAA0001"),
            CrearVehiculo("vehiculo02", "AAA0002", activo: false));

        var result = await _repository.GetAllAsync();

        Assert.That(result.Select(v => v.Id), Is.EqualTo(new[] { "vehiculo01" }));
    }

    [Test]
    public async Task GetPaginatedVehiculosAsync_DevuelvePrimeraPaginaYMetadatos()
    {
        await AddVehiculosAsync(
            CrearVehiculo("vehiculo01", "AAA0001"),
            CrearVehiculo("vehiculo02", "AAA0002"),
            CrearVehiculo("vehiculo03", "AAA0003"),
            CrearVehiculo("vehiculo04", "AAA0004", activo: false));

        var (items, count, totalPages, hasPreviousPage, hasNextPage) =
            await _repository.GetPaginatedVehiculosAsync(1, 2);

        Assert.Multiple(() =>
        {
            Assert.That(items.Select(v => v.Id), Is.EqualTo(new[] { "vehiculo01", "vehiculo02" }));
            Assert.That(count, Is.EqualTo(3));
            Assert.That(totalPages, Is.EqualTo(2));
            Assert.That(hasPreviousPage, Is.False);
            Assert.That(hasNextPage, Is.True);
        });
    }

    [Test]
    public async Task GetPaginatedVehiculosAsync_DevuelveUltimaPaginaYMetadatos()
    {
        await AddVehiculosAsync(
            CrearVehiculo("vehiculo01", "AAA0001"),
            CrearVehiculo("vehiculo02", "AAA0002"),
            CrearVehiculo("vehiculo03", "AAA0003"));

        var (items, count, totalPages, hasPreviousPage, hasNextPage) =
            await _repository.GetPaginatedVehiculosAsync(2, 2);

        Assert.Multiple(() =>
        {
            Assert.That(items.Select(v => v.Id), Is.EqualTo(new[] { "vehiculo03" }));
            Assert.That(count, Is.EqualTo(3));
            Assert.That(totalPages, Is.EqualTo(2));
            Assert.That(hasPreviousPage, Is.True);
            Assert.That(hasNextPage, Is.False);
        });
    }

    [Test]
    public async Task GetPaginatedVehiculosAsync_SinVehiculosActivos_DevuelvePaginaVacia()
    {
        await AddVehiculosAsync(CrearVehiculo("vehiculo01", "AAA0001", activo: false));

        var (items, count, totalPages, hasPreviousPage, hasNextPage) =
            await _repository.GetPaginatedVehiculosAsync(1, 10);

        Assert.Multiple(() =>
        {
            Assert.That(items, Is.Empty);
            Assert.That(count, Is.Zero);
            Assert.That(totalPages, Is.Zero);
            Assert.That(hasPreviousPage, Is.False);
            Assert.That(hasNextPage, Is.False);
        });
    }

    [Test]
    public async Task GetPaginatedVehiculosAsync_ConTokenCancelado_LanzaOperationCanceledException()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _repository.GetPaginatedVehiculosAsync(1, 10, cancellation.Token));
    }

    [Test]
    public async Task GetByIdAsync_ConIdExistente_DevuelveVehiculo()
    {
        await AddVehiculosAsync(CrearVehiculo("vehiculo01", "AAA0001"));

        var result = await _repository.GetByIdAsync("vehiculo01");

        Assert.That(result?.Matricula, Is.EqualTo("AAA0001"));
    }

    [Test]
    public async Task GetByIdAsync_ConIdInexistente_DevuelveNull()
    {
        var result = await _repository.GetByIdAsync("no-existe");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_ConVehiculoInactivo_DevuelveNull()
    {
        await AddVehiculosAsync(CrearVehiculo("vehiculo01", "AAA0001", activo: false));

        var result = await _repository.GetByIdAsync("vehiculo01");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByMatriculaAsync_ConMatriculaExistente_DevuelveVehiculo()
    {
        await AddVehiculosAsync(CrearVehiculo("vehiculo01", "AAA0001"));

        var result = await _repository.GetByMatriculaAsync("AAA0001");

        Assert.That(result?.Id, Is.EqualTo("vehiculo01"));
    }

    [Test]
    public async Task GetByMatriculaAsync_ConMatriculaInexistente_DevuelveNull()
    {
        var result = await _repository.GetByMatriculaAsync("ZZZ9999");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByMatriculaAsync_ConVehiculoInactivo_DevuelveNull()
    {
        await AddVehiculosAsync(CrearVehiculo("vehiculo01", "AAA0001", activo: false));

        var result = await _repository.GetByMatriculaAsync("AAA0001");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_GuardaVehiculoConIdEstadoYFechas()
    {
        var vehiculo = CrearVehiculo(null!, "AAA0001", activo: false);

        var result = await _repository.CreateAsync(vehiculo);
        var stored = await _context.Vehiculos.AsNoTracking().SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.Not.Empty);
            Assert.That(result.Id, Has.Length.EqualTo(11));
            Assert.That(result.Activo, Is.True);
            Assert.That(result.CreatedAt, Is.Not.Null);
            Assert.That(result.UpdatedAt, Is.Not.Null);
            Assert.That(stored.Id, Is.EqualTo(result.Id));
            Assert.That(stored.Activo, Is.True);
        });
    }

    [Test]
    public async Task UpdateAsync_ConVehiculoExistente_PersisteCambios()
    {
        var createdAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var existing = CrearVehiculo("vehiculo01", "AAA0001");
        existing.CreatedAt = createdAt;
        await AddVehiculosAsync(existing);
        var updated = CrearVehiculo("vehiculo01", "AAA0001");
        updated.Color = "Azul";

        var result = await _repository.UpdateAsync(updated);
        var stored = await _context.Vehiculos.AsNoTracking().SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result?.Color, Is.EqualTo("Azul"));
            Assert.That(result?.UpdatedAt, Is.Not.Null);
            Assert.That(result?.CreatedAt, Is.EqualTo(createdAt));
            Assert.That(stored.Color, Is.EqualTo("Azul"));
            Assert.That(stored.CreatedAt, Is.EqualTo(createdAt));
        });
    }

    [Test]
    public async Task UpdateAsync_ConVehiculoInexistente_DevuelveNull()
    {
        var result = await _repository.UpdateAsync(CrearVehiculo("no-existe", "AAA0001"));

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_ConVehiculoInactivo_DevuelveNull()
    {
        await AddVehiculosAsync(CrearVehiculo("vehiculo01", "AAA0001", activo: false));

        var result = await _repository.UpdateAsync(CrearVehiculo("vehiculo01", "AAA0001"));

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_ConVehiculoExistente_RealizaBajaLogica()
    {
        await AddVehiculosAsync(CrearVehiculo("vehiculo01", "AAA0001"));

        var result = await _repository.DeleteAsync("vehiculo01");
        var stored = await _context.Vehiculos.IgnoreQueryFilters()
            .AsNoTracking()
            .SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(stored.Activo, Is.False);
            Assert.That(stored.DeletedAt, Is.Not.Null);
            Assert.That(stored.UpdatedAt, Is.Not.Null);
        });
    }

    [Test]
    public async Task DeleteAsync_ConVehiculoInexistente_DevuelveFalse()
    {
        var result = await _repository.DeleteAsync("no-existe");

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteAsync_ConVehiculoInactivo_DevuelveFalse()
    {
        await AddVehiculosAsync(CrearVehiculo("vehiculo01", "AAA0001", activo: false));

        await _repository.DeleteAsync("vehiculo01");
        
        var result = await _repository.DeleteAsync("vehiculo01");

        Assert.That(result, Is.False);
    }

    private async Task AddVehiculosAsync(params Vehiculo[] vehiculos)
    {
        await _context.Vehiculos.AddRangeAsync(vehiculos);
        await _context.SaveChangesAsync();
    }

    private static Vehiculo CrearVehiculo(string id, string matricula, bool activo = true) => new()
    {
        Id = id,
        Matricula = matricula,
        Marca_id = 1,
        Modelo = "GOLF",
        Acriss = "ECMR",
        categoria = CategoriaEnum.Sedan,
        Concesionario_id = 1,
        Color = "Blanco",
        Combustible = CombustibleEnum.Gasolina,
        Maletero = 400,
        NumeroPlazas = 5,
        Autonomia = 600,
        Imagenes = 1,
        Estado = EstadoEnum.Disponible,
        UltimaRevision = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        Activo = activo
    };
}