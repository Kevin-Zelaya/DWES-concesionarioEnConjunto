using System.Net;
using APIAlquilerVehiculos.Dtos.Taller;
using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Models.Enums;
using APIAlquilerVehiculos.Models.Reservas;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Repositories.Taller;
using APIAlquilerVehiculos.Services.Taller;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace APIAlquilerVehiculos.Test.Services.Test.Taller;

[TestFixture]
public class ParteServiceTests
{
    protected Mock<IParteRepository> _repositoryMock = null!;
    protected Mock<ILogger<ParteService>> _loggerMock = null!;
    protected ParteService _service = null!;

    protected Trabajador _mecanicoActivo = null!;
    protected Vehiculo _vehiculoDisponible = null!;
    protected Parte _parteAbierto = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IParteRepository>();
        _loggerMock = new Mock<ILogger<ParteService>>();
        _service = new ParteService(_repositoryMock.Object, _loggerMock.Object);

        _mecanicoActivo = new Trabajador
        {
            Id = 1,
            Nombre = "Carlos",
            Apellidos = "Ruiz",
            IsActivo = true,
            TipoTrabajador = TipoTrabajador.Mecanico,
            Dni = null 
        };

        _vehiculoDisponible = new Vehiculo
        {
            Id = "veh00000001",
            Matricula = "1234BCD",
            Estado = EstadoEnum.Disponible,
            Activo = true
        };

        _parteAbierto = new Parte
        {
            Id = 10,
            FechaApertura = DateTime.UtcNow.AddHours(-2),
            QuienLoTramitaId = _mecanicoActivo.Id,
            Descripcion = "Revisión de frenos",
            VehiculoId = _vehiculoDisponible.Id,
            Vehiculo = _vehiculoDisponible,
            Estado = EstadoParte.Abierto
        };

        // Configuración por defecto de escenario feliz para minimizar boilerplate en cada test
        _repositoryMock.Setup(r => r.GetTrabajadorByIdAsync(_mecanicoActivo.Id))
            .ReturnsAsync(_mecanicoActivo);
        _repositoryMock.Setup(r => r.GetVehiculoByIdAsync(_vehiculoDisponible.Id))
            .ReturnsAsync(_vehiculoDisponible);
        _repositoryMock.Setup(r => r.TieneParteAbiertoAsync(_vehiculoDisponible.Id))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.GetReservasVivasByVehiculoIdAsync(_vehiculoDisponible.Id))
            .ReturnsAsync([]);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Parte>()))
            .ReturnsAsync((Parte p) =>
            {
                p.Id = 99;
                return p;
            });
        _repositoryMock.Setup(r => r.GetByIdAsync(_parteAbierto.Id))
            .ReturnsAsync(_parteAbierto);
    }

    [TestFixture]
    public class CasosValidos : ParteServiceTests
    {
        [Test]
        public async Task GetAllAsync_CuandoExistenPartesConYSinVehiculoCargado_DevuelveListaMapeada()
        {
            // Arrange
            var parteSinVehiculoNav = new Parte
            {
                Id = 11,
                QuienLoTramitaId = 1,
                Descripcion = "Cambio aceite",
                VehiculoId = "veh00000002",
                Vehiculo = null, // Cubre la rama p.Vehiculo?.Matricula ?? string.Empty
                Estado = EstadoParte.Cerrado
            };
            _repositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync([_parteAbierto, parteSinVehiculoNav]);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
            result.Value[0].MatriculaVehiculo.Should().Be("1234BCD");
            result.Value[1].MatriculaVehiculo.Should().BeEmpty();
        }

        [TestCase(true, "1234BCD")]
        [TestCase(false, "")]
        public async Task GetByIdAsync_CuandoParteExiste_DevuelveDtoConMatricula(bool conVehiculoNav, string matriculaEsperada)
        {
            // Arrange
            if (!conVehiculoNav) _parteAbierto.Vehiculo = null;

            // Act
            var result = await _service.GetByIdAsync(_parteAbierto.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(_parteAbierto.Id);
            result.Value.MatriculaVehiculo.Should().Be(matriculaEsperada);
        }

        [Test]
        public async Task AbrirParteAsync_CuandoDatosSonValidos_CreaParteYDejaVehiculoEnMantenimiento()
        {
            // Arrange
            // Incluye una reserva Finalizada para cubrir el filtro Where(r => EstadosReservaViva.Contains(r.Estado))
            _repositoryMock.Setup(r => r.GetReservasVivasByVehiculoIdAsync(_vehiculoDisponible.Id))
                .ReturnsAsync([new Reserva { CodigoRefencia = "RES-OLD", Estado = Estado.Finalizada }]);

            var request = new AbrirParteRequestDto(_vehiculoDisponible.Id, "  Cambio de neumáticos  ");

            // Act
            var result = await _service.AbrirParteAsync(_mecanicoActivo.Id, request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(99);
            result.Value.Descripcion.Should().Be("Cambio de neumáticos");
            result.Value.Estado.Should().Be(EstadoParte.Abierto);
            _vehiculoDisponible.Estado.Should().Be(EstadoEnum.EnMantenimiento);
            _repositoryMock.Verify(r => r.UpdateVehiculoAsync(_vehiculoDisponible), Times.Once);
        }

        [Test]
        public async Task CerrarParteAsync_CuandoParteEstaAbierto_CierraParteYDevuelveVehiculoADisponible()
        {
            // Arrange
            _vehiculoDisponible.Estado = EstadoEnum.EnMantenimiento;

            // Act
            var result = await _service.CerrarParteAsync(_mecanicoActivo.Id, _parteAbierto.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Estado.Should().Be(EstadoParte.Cerrado);
            result.Value.FechaCierre.Should().NotBeNull();
            _vehiculoDisponible.Estado.Should().Be(EstadoEnum.Disponible);
            _repositoryMock.Verify(r => r.UpdateAsync(_parteAbierto), Times.Once);
            _repositoryMock.Verify(r => r.UpdateVehiculoAsync(_vehiculoDisponible), Times.Once);
        }
    }

    [TestFixture]
    public class CasosInvalidos : ParteServiceTests
    {
        [Test]
        public async Task GetByIdAsync_CuandoParteNoExiste_DevuelveNotFound()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Parte?)null);

            // Act
            var result = await _service.GetByIdAsync(999);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestCase(false, true, true)]   // Trabajador null
        [TestCase(true, false, true)]   // Trabajador inactivo
        [TestCase(true, true, false)]   // Trabajador no es mecánico (Operaciones)
        public async Task AbrirParteAsync_CuandoTrabajadorNoEsMecanicoActivo_DevuelveForbidden(
            bool existe, bool activo, bool esMecanico)
        {
            // Arrange
            Trabajador? trabajador = existe
                ? new Trabajador
                {
                    Id = 2,
                    IsActivo = activo,
                    TipoTrabajador = esMecanico
                        ? TipoTrabajador.Mecanico
                        : TipoTrabajador.Operaciones,
                    Nombre = null,
                    Apellidos = null,
                    Dni = null
                }
                : null;

            _repositoryMock.Setup(r => r.GetTrabajadorByIdAsync(2)).ReturnsAsync(trabajador);

            // Act
            var result = await _service.AbrirParteAsync(2, new AbrirParteRequestDto(_vehiculoDisponible.Id, "Test"));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task AbrirParteAsync_CuandoVehiculoNoExiste_DevuelveNotFound()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetVehiculoByIdAsync("inexistente")).ReturnsAsync((Vehiculo?)null);

            // Act
            var result = await _service.AbrirParteAsync(_mecanicoActivo.Id, new AbrirParteRequestDto("inexistente", "Test"));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [TestCase(false, EstadoEnum.Disponible)] // Activo == false
        [TestCase(true, EstadoEnum.Retirado)]    // Estado == Retirado
        public async Task AbrirParteAsync_CuandoVehiculoEstaInactivoORetirado_DevuelveConflict(bool activo, EstadoEnum estado)
        {
            // Arrange
            _vehiculoDisponible.Activo = activo;
            _vehiculoDisponible.Estado = estado;

            // Act
            var result = await _service.AbrirParteAsync(_mecanicoActivo.Id, new AbrirParteRequestDto(_vehiculoDisponible.Id, "Test"));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [TestCase(EstadoEnum.EnMantenimiento, false)] // Ya en mantenimiento
        [TestCase(EstadoEnum.Disponible, true)]       // Tiene parte abierto previo
        public async Task AbrirParteAsync_CuandoVehiculoYaTieneMantenimientoOParteAbierto_DevuelveConflict(
            EstadoEnum estado, bool tieneParteAbierto)
        {
            // Arrange
            _vehiculoDisponible.Estado = estado;
            _repositoryMock.Setup(r => r.TieneParteAbiertoAsync(_vehiculoDisponible.Id)).ReturnsAsync(tieneParteAbierto);

            // Act
            var result = await _service.AbrirParteAsync(_mecanicoActivo.Id, new AbrirParteRequestDto(_vehiculoDisponible.Id, "Test"));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task AbrirParteAsync_CuandoVehiculoTieneReservasVivas_DevuelveConflict()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetReservasVivasByVehiculoIdAsync(_vehiculoDisponible.Id))
                .ReturnsAsync([new Reserva { CodigoRefencia = "RES-001", Estado = Estado.Confirmada }]);

            // Act
            var result = await _service.AbrirParteAsync(_mecanicoActivo.Id, new AbrirParteRequestDto(_vehiculoDisponible.Id, "Test"));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.StatusCode.Should().Be(HttpStatusCode.Conflict);
            result.Error.Message.Should().Contain("RES-001");
        }

        [TestCase(false, true, true)]   // Trabajador null
        [TestCase(true, false, true)]   // Trabajador inactivo
        [TestCase(true, true, false)]   // Trabajador Operaciones
        public async Task CerrarParteAsync_CuandoTrabajadorNoEsMecanicoActivo_DevuelveForbidden(
            bool existe, bool activo, bool esMecanico)
        {
            // Arrange
            Trabajador? trabajador = existe
                ? new Trabajador
                {
                    Id = 2,
                    IsActivo = activo,
                    TipoTrabajador = esMecanico
                        ? TipoTrabajador.Mecanico
                        : TipoTrabajador.Operaciones,
                    Nombre = null,
                    Apellidos = null,
                    Dni = null
                }
                : null;

            _repositoryMock.Setup(r => r.GetTrabajadorByIdAsync(2)).ReturnsAsync(trabajador);

            // Act
            var result = await _service.CerrarParteAsync(2, _parteAbierto.Id);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Test]
        public async Task CerrarParteAsync_CuandoParteOVehiculoNoExisten_DevuelveNotFound()
        {
            // Arrange (Parte inexistente)
            _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Parte?)null);

            // Act
            var resultParte = await _service.CerrarParteAsync(_mecanicoActivo.Id, 999);

            // Assert
            resultParte.IsFailure.Should().BeTrue();
            resultParte.Error.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // Arrange (Parte existe pero su vehículo ya no existe)
            _repositoryMock.Setup(r => r.GetVehiculoByIdAsync(_parteAbierto.VehiculoId)).ReturnsAsync((Vehiculo?)null);

            // Act
            var resultVehiculo = await _service.CerrarParteAsync(_mecanicoActivo.Id, _parteAbierto.Id);

            // Assert
            resultVehiculo.IsFailure.Should().BeTrue();
            resultVehiculo.Error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task CerrarParteAsync_CuandoParteYaEstaCerrado_DevuelveConflict()
        {
            // Arrange
            _parteAbierto.Estado = EstadoParte.Cerrado;

            // Act
            var result = await _service.CerrarParteAsync(_mecanicoActivo.Id, _parteAbierto.Id);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
    }
}