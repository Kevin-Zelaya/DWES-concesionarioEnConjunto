using APIAlquilerVehiculos.Dtos.Trabajadores;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Validators.Trabajadores;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace APIAlquilerVehiculos.Test.Validators.Test.Trabajadores;

[TestFixture]
public class UpdateTrabajadorDtoValidatorTest
{
    private UpdateTrabajadorDtoValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateTrabajadorDtoValidator();
    }

    private static UpdateTrabajadorDto CrearDtoValido(
        string email = "t@test.com",
        string telefono = "600123456",
        string nombre = "Juan",
        string apellidos = "Pérez",
        string dni = "12345678Z",
        DateTime? fechaIngreso = null,
        TipoTrabajador tipo = TipoTrabajador.Operaciones) =>
        new(
            email,
            telefono,
            nombre,
            apellidos,
            dni,
            fechaIngreso ?? new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            tipo);

    [Test]
    public void Validate_Dto_Valido_No_Tiene_Errores()
    {
        var result = _validator.TestValidate(CrearDtoValido());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_Email_Vacio_O_Invalido_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(email: ""))
            .ShouldHaveValidationErrorFor(x => x.Email);

        _validator.TestValidate(CrearDtoValido(email: "no-es-un-email"))
            .ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Test]
    public void Validate_Telefono_Vacio_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(telefono: ""))
            .ShouldHaveValidationErrorFor(x => x.Telefono);
    }

    [Test]
    public void Validate_Nombre_Vacio_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(nombre: ""))
            .ShouldHaveValidationErrorFor(x => x.Nombre);
    }

    [Test]
    public void Validate_Apellidos_Vacio_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(apellidos: ""))
            .ShouldHaveValidationErrorFor(x => x.Apellidos);
    }

    [Test]
    public void Validate_Dni_Invalido_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(dni: "12345678A"))
            .ShouldHaveValidationErrorFor(x => x.Dni);
    }

    [Test]
    public void Validate_FechaIngreso_Futura_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(fechaIngreso: DateTime.UtcNow.AddDays(1)))
            .ShouldHaveValidationErrorFor(x => x.FechaIngreso);
    }

    [Test]
    public void Validate_TipoTrabajador_Fuera_De_Enum_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(tipo: (TipoTrabajador)999))
            .ShouldHaveValidationErrorFor(x => x.TipoTrabajador);
    }
}
