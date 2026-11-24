using APIAlquilerVehiculos.Dtos.Cientes;
using APIAlquilerVehiculos.Mappers;
using APIAlquilerVehiculos.Models.Usuarios;
using FluentAssertions;

namespace APIAlquilerVehiculos.Test.Mappers.Test.Clientes;

[TestFixture]
public class ClienteMapperTests
{
    #region ToResponseDto

    [Test]
    public void ToResponseDto_DevuelveClienteDtoConTodosLosCamposCuandoUsuarioEstaCargado()
    {
        var fechaNacimiento = new DateTime(1990, 5, 10, 0, 0, 0, DateTimeKind.Utc);
        var createdAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2026, 2, 1, 12, 0, 0, DateTimeKind.Utc);

        var cliente = new Cliente
        {
            Id = 1,
            IdUsuario = 10,
            Nombre = "Juan",
            Apellidos = "Pérez López",
            FechaNacimiento = fechaNacimiento,
            Dni = "12345678A",
            Nie = null,
            Pasaporte = "PAS12345",
            NumeroPermisoConducir = "PERM-001",
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            IsActivo = true,
            DeletedAt = null,
            Usuario = new Usuario
            {
                Id = 10,
                Email = "juan.perez@test.com",
                PhoneNumber = "600123456"
            }
        };

        var dto = cliente.ToResponseDto();

        dto.Should().NotBeNull();
        dto.Id.Should().Be(1);
        dto.IdUsuario.Should().Be(10);
        dto.Nombre.Should().Be("Juan");
        dto.Apellidos.Should().Be("Pérez López");
        dto.Email.Should().Be("juan.perez@test.com");
        dto.Telefono.Should().Be("600123456");
        dto.FechaNacimiento.Should().Be(fechaNacimiento);
        dto.Dni.Should().Be("12345678A");
        dto.Nie.Should().BeNull();
        dto.Pasaporte.Should().Be("PAS12345");
        dto.NumeroPermisoConducir.Should().Be("PERM-001");
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
        dto.IsActivo.Should().BeTrue();
        dto.DeletedAt.Should().BeNull();
    }

    [Test]
    public void ToResponseDto_DevuelveEmailVacioYTelefonoNullCuandoUsuarioEsNull()
    {
        var cliente = new Cliente
        {
            Id = 2,
            IdUsuario = 20,
            Nombre = "Ana",
            Apellidos = "García Ruiz",
            IsActivo = true,
            Usuario = null
        };

        var dto = cliente.ToResponseDto();

        dto.Email.Should().BeEmpty();
        dto.Telefono.Should().BeNull();
    }

    [Test]
    public void ToResponseDto_DevuelveEmailVacioCuandoUsuarioTieneEmailNull()
    {
        var cliente = new Cliente
        {
            Id = 3,
            IdUsuario = 30,
            Nombre = "Carlos",
            Apellidos = "Sánchez",
            IsActivo = false,
            DeletedAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            Usuario = new Usuario
            {
                Id = 30,
                Email = null,
                PhoneNumber = null
            }
        };

        var dto = cliente.ToResponseDto();

        dto.Email.Should().BeEmpty();
        dto.Telefono.Should().BeNull();
        dto.IsActivo.Should().BeFalse();
        dto.DeletedAt.Should().NotBeNull();
    }

    #endregion

    #region UpdateEntity

    [Test]
    public void UpdateEntity_Devuelve_Cliente_CamposRecortados_Docs_NormalizadosAMayusculas()
    {
        var fechaNacimiento = new DateTime(1995, 8, 20, 0, 0, 0, DateTimeKind.Utc);
        var dto = new UpdateClienteDto
        (
            Id :5,
            Nombre : "  Ana María  ",
            Apellidos : "  García Ruiz  ",
            Email: "hgjhgj@gmail.com",
            Telefono : "555555555",
            FechaNacimiento : fechaNacimiento,
            Dni : "  87654321b  ",
            Nie : "  x1234567l  ",
            Pasaporte : "  pas98765  ",
            NumeroPermisoConducir : "  perm-002  "
        );

        var entity = dto.UpdateEntity(5);

        entity.Should().NotBeNull();
        entity.Id.Should().Be(5);
        entity.Nombre.Should().Be("Ana María");
        entity.Apellidos.Should().Be("García Ruiz");
        entity.FechaNacimiento.Should().Be(fechaNacimiento);
        entity.Dni.Should().Be("87654321B");
        entity.Nie.Should().Be("X1234567L");
        entity.Pasaporte.Should().Be("PAS98765");
        entity.NumeroPermisoConducir.Should().Be("PERM-002");
        entity.IsActivo.Should().BeTrue();
    }

    [Test]
    public void UpdateEntity_Devuelve_Docs_Null_Vacios_ConEspacios()
    {
        var dto = new UpdateClienteDto
        (
            Id :9,
            Nombre : "Carlos",
            Apellidos : "Sánchez",
            Email: "hgjhgj@gmail.com",
            Telefono : "555555555",
            FechaNacimiento : new DateTime(1988, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            Dni : null,
            Nie : "",
            Pasaporte : "   ",
            NumeroPermisoConducir : "  \t  "
        );

        var entity = dto.UpdateEntity(9);

        entity.Id.Should().Be(9);
        entity.Dni.Should().BeNull();
        entity.Nie.Should().BeNull();
        entity.Pasaporte.Should().BeNull();
        entity.NumeroPermisoConducir.Should().BeNull();
    }

    #endregion

    #region ToDtoList

    [Test]
    public void ToDtoList_DevuelveListaDeClienteDtoMapeadaCorrectamente()
    {
        var clientes = new List<Cliente>
        {
            new()
            {
                Id = 1,
                IdUsuario = 1,
                Nombre = "Juan",
                Apellidos = "Pérez",
                IsActivo = true,
                Usuario = new Usuario { Email = "juan@test.com" }
            },
            new()
            {
                Id = 2,
                IdUsuario = 2,
                Nombre = "Ana",
                Apellidos = "García",
                IsActivo = true,
                Usuario = new Usuario { Email = "ana@test.com" }
            }
        };

        var result = clientes.ToDtoList();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(1);
        result[0].Email.Should().Be("juan@test.com");
        result[1].Id.Should().Be(2);
        result[1].Email.Should().Be("ana@test.com");
    }

    [Test]
    public void ToDtoList_DevuelveListaVaciaCuandoLaColeccionDeEntradaEstaVacia()
    {
        var clientes = Enumerable.Empty<Cliente>();

        var result = clientes.ToDtoList();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion
}