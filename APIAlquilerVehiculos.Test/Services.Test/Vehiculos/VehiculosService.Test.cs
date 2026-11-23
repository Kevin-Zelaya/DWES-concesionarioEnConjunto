using APIAlquilerVehiculos.Dtos.Vehiculo;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Repositories.Taller;
using APIAlquilerVehiculos.Repositories.Vehiculos;
using APIAlquilerVehiculos.Services.Vehiculo;
using Moq;

namespace APIAlquilerVehiculos.Test.Services.Test.Vehiculos;

[TestFixture]
public class VehiculoServiceTests
{
    private Mock<ICache> _cacheMock = null!;
    private Mock<IVehiculosRepository> _repositoryMock = null!;
    private Mock<IParteRepository> _parteRepositoryMock = null!;

    private VehiculoService _service = null!;

    [SetUp]
    public void Setup()
    {
        _cacheMock = new Mock<ICache>();
        _repositoryMock = new Mock<IVehiculosRepository>();
        _parteRepositoryMock = new Mock<IParteRepository>();

        _service = new VehiculoService(
            _cacheMock.Object,
            _repositoryMock.Object,
            _parteRepositoryMock.Object
        );
    }

    private Vehiculo CreateVehiculo()
    {
        return new Vehiculo
        {
            Id = "ABCDEFGHIJK",
            Matricula = "1234ABC",
            Modelo = "Corolla",
            Acriss = "CDMR",
            categoria = CategoriaEnum.Sedan,
            Concesionario_id = 1,
            precioPorDia = 50,
            Color = "Rojo",
            Combustible = CombustibleEnum.Gasolina,
            Maletero = 450,
            NumeroPlazas = 5,
            Autonomia = 700,
            Imagenes = 1,
            Estado = EstadoEnum.Disponible,
            UltimaRevision = DateTime.UtcNow,
            Activo = true,
            Marca = new Marca
            {
                Id = 1,
                Nombre = "Toyota",
                Wb = "https://toyota.com",
                Pais = "Japon"
            },
            MarcaId = 1
        };
    }

    [Test]
    public async Task GetAllAsync_ReturnsVehicles()
    {
        var data = new List<Vehiculo>
        {
            CreateVehiculo()
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(data);

        var result = await _service.GetAllAsync();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsFromCache()
    {
        var vehiculo = CreateVehiculo();

        _cacheMock
            .Setup(x => x.GetTAsync<Vehiculo>(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var result =
            await _service.GetByIdAsync(vehiculo.Id);

        Assert.That(result.IsSuccess, Is.True);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task GetByIdAsync_ReturnsFromRepository()
    {
        var vehiculo = CreateVehiculo();

        _cacheMock
            .Setup(x => x.GetTAsync<Vehiculo>(vehiculo.Id))
            .ReturnsAsync((Vehiculo?)null);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var result =
            await _service.GetByIdAsync(vehiculo.Id);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task GetByIdAsync_ReturnsFailure_WhenNotFound()
    {
        _cacheMock
            .Setup(x => x.GetTAsync<Vehiculo>(It.IsAny<string>()))
            .ReturnsAsync((Vehiculo?)null);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Vehiculo?)null);

        var result =
            await _service.GetByIdAsync("NOEXISTE");

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task GetByMatriculaAsync_ReturnsVehiculo()
    {
        var vehiculo = CreateVehiculo();

        _repositoryMock
            .Setup(x => x.GetByMatriculaAsync(vehiculo.Matricula))
            .ReturnsAsync(vehiculo);

        var result =
            await _service.GetByMatriculaAsync(
                vehiculo.Matricula);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task GetByMatriculaAsync_ReturnsFailure_WhenNotFound()
    {
        _repositoryMock
            .Setup(x => x.GetByMatriculaAsync(It.IsAny<string>()))
            .ReturnsAsync((Vehiculo?)null);

        var result =
            await _service.GetByMatriculaAsync("NOEXISTE");

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task CreateAsync_ReturnsFailure_WhenMatriculaExists()
    {
        var vehiculo = CreateVehiculo();

        _repositoryMock
            .Setup(x => x.GetByMatriculaAsync(
                vehiculo.Matricula))
            .ReturnsAsync(vehiculo);

        var dto = new VehiculoCreateDto
        {
            Matricula = vehiculo.Matricula
        };

        var result =
            await _service.CreateAsync(dto);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task UpdateAsync_ReturnsFailure_WhenVehicleNotExists()
    {
        _repositoryMock
            .Setup(x => x.GetByMatriculaAsync(It.IsAny<string>()))
            .ReturnsAsync((Vehiculo?)null);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Vehiculo?)null);

        var dto = new VehiculoUpdateDto();

        var result =
            await _service.UpdateAsync(
                "NOEXISTE",
                dto);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task DeleteAsync_ReturnsFailure_WhenVehicleNotExists()
    {
        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Vehiculo?)null);

        var result =
            await _service.DeleteAsync("NOEXISTE");

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task DeleteAsync_ReturnsFailure_WhenVehicleNotDisponible()
    {
        var vehiculo = CreateVehiculo();
        vehiculo.Estado = EstadoEnum.EnAlquiler;

        _repositoryMock
            .Setup(x => x.GetByIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        var result =
            await _service.DeleteAsync(vehiculo.Id);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task DeleteAsync_ReturnsFailure_WhenOpenParteExists()
    {
        var vehiculo = CreateVehiculo();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        _parteRepositoryMock
            .Setup(x => x.TieneParteAbiertoAsync(
                vehiculo.Id))
            .ReturnsAsync(true);

        var result =
            await _service.DeleteAsync(vehiculo.Id);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task DeleteAsync_ReturnsSuccess()
    {
        var vehiculo = CreateVehiculo();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        _parteRepositoryMock
            .Setup(x => x.TieneParteAbiertoAsync(
                vehiculo.Id))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(x => x.DeleteAsync(vehiculo.Id))
            .ReturnsAsync(true);

        var result =
            await _service.DeleteAsync(vehiculo.Id);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public async Task GetPaginatedVehiculosAsync_ReturnsFailure_WhenParamsInvalid()
    {
        var result =
            await _service.GetPaginatedVehiculosAsync(
                0,
                10);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task UpdateEstado_ReturnsFailure_WhenVehicleNotExists()
    {
        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Vehiculo?)null);

        var result =
            await _service.UpdateEstado(
                "NOEXISTE",
                EstadoEnum.EnAlquiler);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task UpdateEstado_ReturnsFailure_WhenReservadoAndOpenParte()
    {
        var vehiculo = CreateVehiculo();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        _parteRepositoryMock
            .Setup(x => x.TieneParteAbiertoAsync(
                vehiculo.Id))
            .ReturnsAsync(true);

        var result =
            await _service.UpdateEstado(
                vehiculo.Id,
                EstadoEnum.Reservado);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task UpdateEstado_ReturnsSuccess()
    {
        var vehiculo = CreateVehiculo();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(vehiculo.Id))
            .ReturnsAsync(vehiculo);

        _parteRepositoryMock
            .Setup(x => x.TieneParteAbiertoAsync(
                vehiculo.Id))
            .ReturnsAsync(false);

        vehiculo.Estado = EstadoEnum.EnAlquiler;

        _repositoryMock
            .Setup(x => x.UpdateEstado(
                vehiculo.Id,
                EstadoEnum.EnAlquiler))
            .ReturnsAsync(vehiculo);

        var result =
            await _service.UpdateEstado(
                vehiculo.Id,
                EstadoEnum.EnAlquiler);

        Assert.That(result.IsSuccess, Is.True);
    }
}