using APIAlquilerVehiculos.Models;
using APIAlquilerVehiculos.Repositories.Taller;
using FluentAssertions;

namespace APIAlquilerVehiculos.Test.Repository.Test.Taller;

[TestFixture]
public class ParteRepositoryTests
{
    protected ParteRepository _repository = null!;
    protected Parte _parteSemilla = null!;

    [SetUp]
    public async Task SetUp()
    {
        _repository = new ParteRepository();

        _parteSemilla = await _repository.CreateAsync(new Parte
        {
            FechaApertura = DateTime.UtcNow,
            QuienLoTramitaId = 1,
            Descripcion = "Revisión inicial",
            VehiculoId = "veh00000001",
            Estado = EstadoParte.Abierto
        });
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
                QuienLoTramitaId = 1,
                Descripcion = "Cambio de aceite",
                VehiculoId = "veh00000001",
                Estado = EstadoParte.Abierto
            };

            // Act (Create & GetAll)
            var creado = await _repository.CreateAsync(nuevoParte);
            var todos = await _repository.GetAllAsync();

            // Assert (Create & GetAll)
            creado.Id.Should().Be(2);
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
            var resultado = await _repository.TieneParteAbiertoAsync("veh00000001");

            // Assert
            resultado.Should().Be(esperado);
        }

        [Test]
        public async Task ConsultasApoyo_TrabajadorVehiculoReservasYUpdateVehiculo_DevuelvenDatosSemilla()
        {
            // Arrange
            var vehiculo = await _repository.GetVehiculoByIdAsync("veh00000001");

            // Act
            var trabajador = await _repository.GetTrabajadorByIdAsync(1);
            var reservas = await _repository.GetReservasVivasByVehiculoIdAsync("veh00000001");
            await _repository.UpdateVehiculoAsync(vehiculo!);

            // Assert
            trabajador.Should().NotBeNull();
            trabajador!.Id.Should().Be(1);
            vehiculo.Should().NotBeNull();
            vehiculo!.Matricula.Should().Be("1234BCD");
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
            var parteInexistente = new Parte { Id = 999, Descripcion = "No existe" };

            // Act
            var obtenido = await _repository.GetByIdAsync(999);
            var actualizado = await _repository.UpdateAsync(parteInexistente);
            var eliminado = await _repository.DeleteAsync(999);

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
            const long trabajadorInexistente = 999;

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