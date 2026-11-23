using APIAlquilerVehiculos.Dtos.Cientes;
using APIAlquilerVehiculos.Validators.Clientes;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace APIAlquilerVehiculos.Test.Validators.Test.Clientes;

[TestFixture]
public class UpdateClienteValidatorTest
{
    private UpdateClienteValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateClienteValidator();
    }

    // Helper para instanciar UpdateClienteDto válido por defecto y modificar solo lo que cada test necesita
    private static UpdateClienteDto CrearDtoValido(
        DateTime? fechaNacimiento = null,
        string? dni = "12345678Z",
        string? nie = null,
        string? pasaporte = null,
        string? permisoConducir = null)
    {
        // Ajusta los argumentos si tu record UpdateClienteDto tiene distinto orden o parámetros extra
        return new UpdateClienteDto(
            Id: 2,
            Nombre: "Juan",
            Apellidos: "Pérez López",
            Email: "juan@test.com",
            Telefono: "600123456",
            FechaNacimiento: fechaNacimiento ?? new DateTime(1990, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            Dni: dni,
            Nie: nie,
            Pasaporte: pasaporte,
            NumeroPermisoConducir: permisoConducir
        );
    }

    #region FechaNacimiento

    [Test]
    public void Validate_Todos_Campos_Validos_ConDni()
    {
        var dto = CrearDtoValido(dni: "12345678Z", nie: null, pasaporte: null, permisoConducir: null);

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_Devuelve_Error_FechaNacimiento_Default()
    {
        var dto = CrearDtoValido(fechaNacimiento: default(DateTime));

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FechaNacimiento)
              .WithErrorMessage("La fecha de nacimiento es obligatoria.");
    }

    [Test]
    public void Validate_Devuelve_Error_FechaNacimiento_Hoy_Futura()
    {
        var dtoHoy = CrearDtoValido(fechaNacimiento: DateTime.UtcNow.Date);
        var dtoFutura = CrearDtoValido(fechaNacimiento: DateTime.UtcNow.AddDays(5));

        var resultHoy = _validator.TestValidate(dtoHoy);
        var resultFutura = _validator.TestValidate(dtoFutura);

        resultHoy.ShouldHaveValidationErrorFor(x => x.FechaNacimiento)
                 .WithErrorMessage("La fecha de nacimiento debe ser anterior a hoy.");

        resultFutura.ShouldHaveValidationErrorFor(x => x.FechaNacimiento)
                    .WithErrorMessage("La fecha de nacimiento debe ser anterior a hoy.");
    }

    [Test]
    public void Validate_Devuelve_Error_FechaNacimiento_IgualOAnteriorA1900()
    {
        var dto = CrearDtoValido(fechaNacimiento: new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.FechaNacimiento)
              .WithErrorMessage("La fecha de nacimiento no es válida.");
    }

    #endregion

    #region Documentos (DNI, NIE, Pasaporte)

    [Test]
    public void Validate_Devuelve_Error_SinDocs()
    {
        var dto = CrearDtoValido(dni: null, nie: "", pasaporte: "   ");

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor("Documentos")
              .WithErrorMessage("Debe aportar al menos un documento de identidad válido (DNI, NIE o Pasaporte).");
    }

    [Test]
    public void Validate_Devuelve_Error_Dni_FormatoInvalido()
    {
        var dto = CrearDtoValido(dni: "12345678A"); // Letra incorrecta para 12345678 (debería ser Z)

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Dni);
    }

    [Test]
    public void Validate_Todos_Campos_Validos_ConNie()
    {
        var dto = CrearDtoValido(dni: null, nie: "X1234567L", pasaporte: null, permisoConducir: null);

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_Devuelve_Error_Nie_FormatoInvalido()
    {
        var dto = CrearDtoValido(dni: null, nie: "X1234567A"); // Letra incorrecta para X1234567 (debería ser L)

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Nie);
    }

    [Test]
    public void Validate_Devuelve_Error_Pasaporte_Invalido()
    {
        var dto = CrearDtoValido(dni: null, nie: null, pasaporte: "12", permisoConducir: "PERM-001");

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Pasaporte);
    }

    #endregion

    #region Regla Pasaporte + NumeroPermisoConducir

    [Test]
    public void Validate_Todos_Campos_Validos_ConPasaporte_YPermisoConducir()
    {
        var dto = CrearDtoValido(dni: null, nie: null, pasaporte: "PAD9876543", permisoConducir: "PERM-999");

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
    
    
    [Test]
    public void Validate_Devuelve_Error_Pasaporte_SinPermisoDeConducir()
    {
        var dto = CrearDtoValido(dni: null, nie: null, pasaporte: "PAD9876543", permisoConducir: null);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.NumeroPermisoConducir)
              .WithErrorMessage("Si se registra únicamente con pasaporte, debe indicar el número del permiso de conducir.");
    }

    [Test]
    public void Validate_Pasaporte_Sin_PermisoConducir_PeroConDni()
    {
        var dto = CrearDtoValido(dni: "12345678Z", nie: null, pasaporte: "PAA123456", permisoConducir: null);

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.NumeroPermisoConducir);
    }

    #endregion
}