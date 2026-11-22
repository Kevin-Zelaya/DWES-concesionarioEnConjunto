
using APIAlquilerVehiculos.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using APIAlquilerVehiculos.Entities;
using Microsoft.EntityFrameworkCore;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Usuarios;

namespace APIAlquilerVehiculos.Tests.Services;

[TestFixture]
public class ReservaServiceTests
{
    private PostgresFixture _postgres = null!;
    private ServiceProvider _provider = null!;
    private IServiceScope _scope = null!;
    private IReservaService _service = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _postgres = new PostgresFixture();
        await _postgres.StartAsync();

        _provider = TestServices.Build(_postgres.ConnectionString);

        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();
    }

    [SetUp]
    public void SetUp()
    {
        _scope = _provider.CreateScope();

        _service = _scope.ServiceProvider
            .GetRequiredService<IReservaService>();
    }

    [TearDown]
    public void TearDown()
    {
        _scope.Dispose();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _provider.DisposeAsync();
        await _postgres.StopAsync();
    }

    [Test]
    public async Task ExistsAsync_ReservaInexistente_ReturnsFalse()
    {
        // Arrange
        const long id = -1;

        // Act
        var result = await _service.ExistsAsync(id);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.False);
    } 

    [Test]
    public async Task ExistsAsync_ReservaExistente_ReturnsTrue()
    {
        // Arrange
        var context = _scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var reserva = new Reserva
        {
            ClienteId = 1,
            VehiculoId = "vehiculo001",
            Categoria = CategoriaEnum.Sedan,
            FechaCreacion = DateTime.Now,
            FechaRecogida = DateTime.Now.AddDays(1),
            FechaDevolucion = DateTime.Now.AddDays(5),
            Estado = Estado.Pendiente,
            PrecioTotal = 200m
        };

        context.Reservas.Add(reserva);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.ExistsAsync(reserva.Id);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.True);
    }

    [Test]
    public async Task GetReservaByIdAsync_ReservaInexistente_ReturnsFailure()
    {
        // Arrange
        const long id = -1;

        // Act
        var result = await _service.GetReservaByIdAsync(id);

        // Assert
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task GetReservaByCodigoReferenciaAsync_CodigoInexistente_ReturnsFailure()
    {
        // Arrange
        const string codigo = "ZZZZZZ";

        // Act
        var result = await _service.GetReservaByCodigoReferenciaAsync(codigo);

        // Assert
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task GetAllReservasAsync_SinError_ReturnsSuccess()
    {
        // Act
        var result = await _service.GetAllReservasAsync();

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
    }

    [Test]
    public async Task GetPaginatedReservasAsync_ParametrosValidos_ReturnsSuccess()
    {
        // Act
        var result = await _service.GetPaginatedReservasAsync(1, 10);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
    }

    [Test]
    public async Task GetPaginatedReservasByCustomerIdAsync_ClienteInexistente_ReturnsSuccess()
    {
        // Act
        var result = await _service.GetPaginatedReservasByCustomerIdAsync(-1, 1, 10);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
    }

    [Test]
    public async Task GetPaginatedReservasByEmployeeIdAsync_TrabajadorInexistente_ReturnsSuccess()
    {
        // Act
        var result = await _service.GetPaginatedReservasByEmployeeIdAsync(-1, 1, 10);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
    }

    [Test]
    public async Task CreateReservaAsync_ClienteInexistente_ReturnsFailure()
    {
        // Arrange
        var request = new ReservaCreateRequestDto(
            ClienteId: -1,
            Categoria: CategoriaEnum.Suv,
            VehiculoId: "vehiculo001",
            FechaRecogida: new DateTime(2026, 12, 1, 10, 0, 0),
            FechaDevolucion: new DateTime(2026, 12, 5, 10, 0, 0),
            Lineas: []
        );

        // Act
        var result = await _service.CreateReservaAsync(request, -1);

        // Assert
        Assert.That(result.IsFailure, Is.True);
    }
    [Test]
    public async Task UpdateNotaAsync_ReservaInexistente_ReturnsFailure()
    {
        // Arrange
        const string codigo = "ZZZZZZ";
        var request = new ReservaUpdateNotaRequestDto("Nota de prueba", 1);

        // Act
        var result = await _service.UpdateNotaAsync(codigo, request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task UpdatePagoAsync_ReservaInexistente_ReturnsFailure()
    {
        // Arrange
        const string codigo = "ZZZZZZ";
        var request = new ReservaUpdatePagoRequestDto(1, 1);

        // Act
        var result = await _service.UpdatePagoAsync(codigo, request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task UpdateFechasAsync_ReservaInexistente_ReturnsFailure()
    {
        // Arrange
        const string codigo = "ZZZZZZ";
        var request = new ReservaUpdateFechaRequestDto(
            new DateTime(2026, 12, 1, 10, 0, 0),
            new DateTime(2026, 12, 5, 10, 0, 0),
            1
        );

        // Act
        var result = await _service.UpdateFechasAsync(codigo, request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task UpdateStatusAsync_ReservaInexistente_ReturnsFailure()
    {
        // Arrange
        const string codigo = "ZZZZZZ";
        var request = new ReservaUpdateStatusRequestDto(Estado.Cancelada, 1);

        // Act
        var result = await _service.UpdateStatusAsync(codigo, request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task UpdateVehicleAsync_ReservaInexistente_ReturnsFailure()
    {
        // Arrange
        const string codigo = "ZZZZZZ";
        var request = new ReservaUpdateVehicleRequestDto("vehiculo001", 1);

        // Act
        var result = await _service.UpdateVehicleAsync(codigo, request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
    }
    [Test]
    public async Task UpdateStatusAsync_TransicionNoValida_ReturnsFailure()
    {
        // Arrange
        const string codigo = "ABC123";

        var context = _scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var vehiculo = new Vehiculo
        {
            Id = "vehiculo001",
            Modelo = "Seat Ibiza",
            Marca = "Prueba"
        };

        // Crear usuario asociado al cliente
        var usuario = new Usuario
        {
            UserName = "cliente@alquiler.com",
            Email = "cliente@alquiler.com",
            PhoneNumber = "600000005",
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        context.Users.Add(usuario);
        await context.SaveChangesAsync();

        // Asociar usuario y cliente
        var cliente = new Cliente
        {
            Nombre = "Juan",
            Apellidos = "Pérez García",
            FechaNacimiento = new DateTime(1995, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow,
            IsActivo = true,
            IdUsuario = usuario.Id,
            Usuario = usuario
        };

        context.Clientes.Add(cliente);
        context.Vehiculos.Add(vehiculo);
        await context.SaveChangesAsync();

        var reserva = new Reserva
        {
            CodigoRefencia = codigo,
            ClienteId = cliente.Id,
            VehiculoId = vehiculo.Id,
            Estado = Estado.Pendiente,
            FechaCreacion = DateTime.UtcNow,
            FechaRecogida = DateTime.UtcNow.AddDays(1),
            FechaDevolucion = DateTime.UtcNow.AddDays(5)
        };

        context.Reservas.Add(reserva);
        await context.SaveChangesAsync();

        var request = new ReservaUpdateStatusRequestDto(
            Estado.Finalizada,
            reserva.RowVersion
        );

        // Act
        var result = await _service.UpdateStatusAsync(codigo, request);

        // Assert
        Assert.That(result.IsFailure, Is.True);

        var reservaDb = await context.Reservas
            .AsNoTracking()
            .FirstAsync(r => r.CodigoRefencia == codigo);

        Assert.That(reservaDb.Estado, Is.EqualTo(Estado.Pendiente));
    }
    [TestCase(Estado.Pendiente, Estado.Finalizada)]
    [TestCase(Estado.Pendiente, Estado.EnCurso)]
    [TestCase(Estado.Confirmada, Estado.Finalizada)]
    [TestCase(Estado.EnCurso, Estado.Cancelada)]
    [TestCase(Estado.Finalizada, Estado.Confirmada)]
    public void PuedeTransicionarA_TransicionNoValida_ReturnsFalse(
        Estado estadoActual,
        Estado nuevoEstado)
    {
        // Act
        var result = estadoActual.PuedeTransicionarA(nuevoEstado);

        // Assert
        Assert.That(result, Is.False);
    }
    [TestCase(Estado.Pendiente, Estado.Confirmada)]
    [TestCase(Estado.Pendiente, Estado.Cancelada)]
    [TestCase(Estado.Pendiente, Estado.Caducada)]
    [TestCase(Estado.Confirmada, Estado.EnCurso)]
    [TestCase(Estado.Confirmada, Estado.NoPresentado)]
    [TestCase(Estado.EnCurso, Estado.Finalizada)]
    public void PuedeTransicionarA_TransicionValida_ReturnsTrue(
        Estado estadoActual,
        Estado nuevoEstado)
    {
        // Act
        var result = estadoActual.PuedeTransicionarA(nuevoEstado);

        // Assert
        Assert.That(result, Is.True);
    }


}
