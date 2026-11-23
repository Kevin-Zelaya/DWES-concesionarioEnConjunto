using APIAlquilerVehiculos.Dtos.Trabajadores;
using APIAlquilerVehiculos.Models.Usuarios;
using APIAlquilerVehiculos.Validators.Trabajadores;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace APIAlquilerVehiculos.Test.Validators.Test.Trabajadores;

[TestFixture]
public class CreateTrabajadorDtoValidatorTest
{
    private CreateTrabajadorDtoValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new CreateTrabajadorDtoValidator();
    }

    private static CreateTrabajadorDto CrearDtoValido(
        string email = "t@test.com",
        string telefono = "600123456",
        string contraseña = "Password123",
        string nombre = "Juan",
        string apellidos = "Pérez",
        string? dni = "12345678Z",
        DateTime? fechaIngreso = null,
        TipoTrabajador tipo = TipoTrabajador.Operaciones) =>
        new(email, telefono, contraseña, nombre, apellidos, dni!, fechaIngreso, tipo);

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
    public void Validate_Contrasena_Corta_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(contraseña: "Corta1"))
            .ShouldHaveValidationErrorFor(x => x.Contraseña);
    }

    [Test]
    public void Validate_Nombre_Vacio_O_Corto_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(nombre: ""))
            .ShouldHaveValidationErrorFor(x => x.Nombre);

        _validator.TestValidate(CrearDtoValido(nombre: "J"))
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
        // letra de control incorrecta para 12345678 (debería ser Z)
        _validator.TestValidate(CrearDtoValido(dni: "12345678A"))
            .ShouldHaveValidationErrorFor(x => x.Dni);

        _validator.TestValidate(CrearDtoValido(dni: "123"))
            .ShouldHaveValidationErrorFor(x => x.Dni);
    }

    [Test]
    public void Validate_Dni_Null_No_Valida_Formato_Pero_Es_Opcional_En_FluentValidation()
    {
        // el DTO lo marca Required por DataAnnotations, pero el validator solo valida formato si tiene valor
        var result = _validator.TestValidate(CrearDtoValido(dni: null));

        result.ShouldNotHaveValidationErrorFor(x => x.Dni);
    }

    [Test]
    public void Validate_FechaIngreso_Futura_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(fechaIngreso: DateTime.UtcNow.AddDays(1)))
            .ShouldHaveValidationErrorFor(x => x.FechaIngreso);
    }

    [Test]
    public void Validate_FechaIngreso_Pasada_O_Null_No_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(fechaIngreso: new DateTime(2020, 1, 1)))
            .ShouldNotHaveValidationErrorFor(x => x.FechaIngreso);

        _validator.TestValidate(CrearDtoValido(fechaIngreso: null))
            .ShouldNotHaveValidationErrorFor(x => x.FechaIngreso);
    }

    [Test]
    public void Validate_TipoTrabajador_Fuera_De_Enum_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(tipo: (TipoTrabajador)999))
            .ShouldHaveValidationErrorFor(x => x.TipoTrabajador);
    }
}
