using APIAlquilerVehiculos.Dtos.Trabajadores;
using APIAlquilerVehiculos.Mappers;
using APIAlquilerVehiculos.Models.Usuarios;
using FluentAssertions;

namespace APIAlquilerVehiculos.Test.Mappers.Test.Trabajadores;

[TestFixture]
public class TrabajadorMapperTests
{
    protected Usuario _usuarioBase = null!;
    protected Trabajador _trabajadorBase = null!;

    [SetUp]
    public void SetUp()
    {
        _usuarioBase = new Usuario
        {
            Id = 10,
            Email = "carlos.mecanico@empresa.es",
            PhoneNumber = "600123456",
            CreatedAt = DateTime.UtcNow
        };

        _trabajadorBase = new Trabajador
        {
            Id = 1,
            IdUsuario = _usuarioBase.Id,
            Usuario = _usuarioBase,
            Nombre = "Carlos",
            Apellidos = "Gómez",
            Dni = "12345678Z",
            FechaIngreso = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            TipoTrabajador = TipoTrabajador.Mecanico,
            IsActivo = true,
            CreatedAt = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = null
        };
    }

    [TestFixture]
    public class CasosValidos : TrabajadorMapperTests
    {
        [TestCase(true, "carlos.mecanico@empresa.es", "600123456")]
        [TestCase(false, "", null)]
        public void ToResponseDto_ConYSinUsuarioCargado_MapeaCorrectamente(
            bool conUsuario, string emailEsperado, string? telefonoEsperado)
        {
            // Arrange
            if (!conUsuario)
            {
                _trabajadorBase = new Trabajador
                {
                    Id = _trabajadorBase.Id,
                    IdUsuario = _trabajadorBase.IdUsuario,
                    Usuario = null!,
                    Nombre = _trabajadorBase.Nombre,
                    Apellidos = _trabajadorBase.Apellidos,
                    Dni = _trabajadorBase.Dni,
                    FechaIngreso = _trabajadorBase.FechaIngreso,
                    TipoTrabajador = _trabajadorBase.TipoTrabajador,
                    IsActivo = _trabajadorBase.IsActivo,
                    CreatedAt = _trabajadorBase.CreatedAt
                };
            }

            // Act
            var dto = _trabajadorBase.ToResponseDto();

            // Assert
            dto.Id.Should().Be(1);
            dto.IdUsuario.Should().Be(10);
            dto.Nombre.Should().Be("Carlos");
            dto.Apellidos.Should().Be("Gómez");
            dto.Email.Should().Be(emailEsperado);
            dto.Telefono.Should().Be(telefonoEsperado);
            dto.Dni.Should().Be("12345678Z");
            dto.TipoTrabajador.Should().Be("Mecanico");
            dto.IsActivo.Should().BeTrue();
        }

        [TestCase(DateTimeKind.Utc, true)]
        [TestCase(DateTimeKind.Local, true)]
        [TestCase(DateTimeKind.Unspecified, false)]
        public void ToEntity_NormalizaCamposYConvierteFechaAUtdCorrectamente(
            DateTimeKind kind, bool conFechaExplicita)
        {
            // Arrange
            DateTime? fechaDto = conFechaExplicita
                ? new DateTime(2026, 5, 10, 10, 0, 0, kind)
                : null;

            var dto = new CreateTrabajadorDto(
                Email: "nuevo@empresa.es",
                Telefono: "611223344",
                Contraseña: "Password123!",
                Nombre: "  Laura  ",
                Apellidos: "  Pérez  ",
                Dni: "  12345678z  ",
                FechaIngreso: fechaDto,
                TipoTrabajador: TipoTrabajador.Operaciones);

            // Act
            var entidad = dto.ToEntity(_usuarioBase);

            // Assert
            entidad.IdUsuario.Should().Be(_usuarioBase.Id);
            entidad.Usuario.Should().Be(_usuarioBase);
            entidad.Nombre.Should().Be("Laura");
            entidad.Apellidos.Should().Be("Pérez");
            entidad.Dni.Should().Be("12345678Z");
            entidad.FechaIngreso.Kind.Should().Be(DateTimeKind.Utc);
            entidad.TipoTrabajador.Should().Be(TipoTrabajador.Operaciones);
            entidad.IsActivo.Should().BeTrue();
        }

        [Test]
        public void UpdateEntity_NormalizaDatosYConvierteFechaUnspecifiedAUtd()
        {
            // Arrange
            var fechaUnspecified = new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Unspecified);
            var dto = new UpdateTrabajadorDto(
                Email: "editado@empresa.es",
                Telefono: "699887766",
                Nombre: "  Mario  ",
                Apellidos: "  López  ",
                Dni: "  87654321x  ",
                FechaIngreso: fechaUnspecified,
                TipoTrabajador: TipoTrabajador.Mecanico);

            // Act
            var entidad = dto.UpdateEntity(id: 42);

            // Assert
            entidad.Id.Should().Be(42);
            entidad.Nombre.Should().Be("Mario");
            entidad.Apellidos.Should().Be("López");
            entidad.Dni.Should().Be("87654321X");
            entidad.FechaIngreso.Kind.Should().Be(DateTimeKind.Utc);
            entidad.TipoTrabajador.Should().Be(TipoTrabajador.Mecanico);
            entidad.IsActivo.Should().BeTrue();
        }

        [TestCase(TipoTrabajador.Operaciones, Roles.Operario)]
        [TestCase(TipoTrabajador.Mecanico, Roles.Mecanico)]
        public void ToRol_CuandoTipoEsValido_DevuelveRolCorrespondiente(
            TipoTrabajador tipo, string rolEsperado)
        {
            // Arrange & Act
            var rol = tipo.ToRol();

            // Assert
            rol.Should().Be(rolEsperado);
        }
    }

    [TestFixture]
    public class CasosInvalidos : TrabajadorMapperTests
    {
        [Test]
        public void ToRol_CuandoTipoTrabajadorNoEstaSoportado_LanzaArgumentOutOfRangeException()
        {
            // Arrange
            var tipoInvalido = (TipoTrabajador)999;

            // Act
            Action act = () => tipoInvalido.ToRol();

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("tipo");
        }
    }
}