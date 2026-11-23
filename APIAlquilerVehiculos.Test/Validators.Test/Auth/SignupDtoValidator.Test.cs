using APIAlquilerVehiculos.Dtos.Auth;
using FluentAssertions;
using FluentValidation.TestHelper;
using AuthSignupDtoValidator = APIAlquilerVehiculos.Validators.Auth.SignupDtoValidator;

namespace APIAlquilerVehiculos.Test.Validators.Test.Auth;

[TestFixture]
public class SignupDtoValidatorTest
{
    // OJO: existe una clase SignupDtoValidator duplicada en el namespace global
    // (Validators/Reservas/ReservaUpdateFechasDtoValidator.cs), hay que usar el alias.
    private AuthSignupDtoValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new AuthSignupDtoValidator();
    }

    private static SignupDto CrearDtoValido(
        string contraseña = "Password123",
        DateTime? fechaNacimiento = null,
        string? dni = "12345678Z",
        string? nie = null,
        string? pasaporte = null,
        string? permiso = null) =>
        new(
            "t@test.com",
            "600123456",
            contraseña,
            "Juan",
            "Pérez",
            fechaNacimiento ?? new DateTime(1990, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            dni,
            nie,
            pasaporte,
            permiso);

    [Test]
    public void Validate_Con_Dni_Valido_No_Tiene_Errores()
    {
        var result = _validator.TestValidate(CrearDtoValido());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_Con_Nie_Valido_No_Tiene_Errores()
    {
        var result = _validator.TestValidate(CrearDtoValido(dni: null, nie: "X1234567L"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_Con_Pasaporte_Y_PermisoConducir_No_Tiene_Errores()
    {
        var result = _validator.TestValidate(CrearDtoValido(
            dni: null,
            pasaporte: "PAD9876543",
            permiso: "PERM-001"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_Contrasena_Sin_Mayuscula_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(contraseña: "password123"))
            .ShouldHaveValidationErrorFor(x => x.Contraseña);
    }

    [Test]
    public void Validate_Contrasena_Sin_Minucula_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(contraseña: "PASSWORD123"))
            .ShouldHaveValidationErrorFor(x => x.Contraseña);
    }

    [Test]
    public void Validate_Contrasena_Sin_Numero_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(contraseña: "Password"))
            .ShouldHaveValidationErrorFor(x => x.Contraseña);
    }

    [Test]
    public void Validate_FechaNacimiento_Default_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(fechaNacimiento: DateTime.MinValue))
            .ShouldHaveValidationErrorFor(x => x.FechaNacimiento);
    }

    [Test]
    public void Validate_FechaNacimiento_Futura_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(fechaNacimiento: DateTime.UtcNow.AddDays(1)))
            .ShouldHaveValidationErrorFor(x => x.FechaNacimiento);
    }

    [Test]
    public void Validate_FechaNacimiento_Anterior_A_1900_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(fechaNacimiento: new DateTime(1899, 12, 31)))
            .ShouldHaveValidationErrorFor(x => x.FechaNacimiento);
    }

    [Test]
    public void Validate_Sin_Ningun_Documento_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(dni: null, nie: null, pasaporte: null))
            .ShouldHaveValidationErrorFor("Documentos");
    }

    [Test]
    public void Validate_Dni_Invalido_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(dni: "12345678A"))
            .ShouldHaveValidationErrorFor(x => x.Dni);
    }

    [Test]
    public void Validate_Nie_Invalido_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(dni: null, nie: "X1234567A"))
            .ShouldHaveValidationErrorFor(x => x.Nie);
    }

    [Test]
    public void Validate_Pasaporte_Invalido_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(dni: null, pasaporte: "12", permiso: "PERM-001"))
            .ShouldHaveValidationErrorFor(x => x.Pasaporte);
    }

    [Test]
    public void Validate_Solo_Pasaporte_Sin_PermisoConducir_Tiene_Error()
    {
        _validator.TestValidate(CrearDtoValido(dni: null, pasaporte: "PAD9876543", permiso: null))
            .ShouldHaveValidationErrorFor(x => x.NumeroPermisoConducir);
    }

    [Test]
    public void Validate_Pasaporte_Con_Dni_Sin_PermisoConducir_No_Tiene_Error()
    {
        var result = _validator.TestValidate(CrearDtoValido(
            dni: "12345678Z",
            pasaporte: "PAD9876543",
            permiso: null));

        result.ShouldNotHaveValidationErrorFor(x => x.NumeroPermisoConducir);
    }
}
