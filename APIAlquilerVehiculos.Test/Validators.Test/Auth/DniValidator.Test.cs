using APIAlquilerVehiculos.Validators.Auth.Documentos;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace APIAlquilerVehiculos.Test.Validators.Test.Auth;

[TestFixture]
public class DniValidatorTest
{
    private DniValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new DniValidator();
    }

    [Test]
    public void Validate_Dni_Valido_No_Tiene_Errores()
    {
        _validator.TestValidate("12345678Z").ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_Dni_En_Minusculas_Tambien_Es_Valido()
    {
        _validator.TestValidate("12345678z").ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_Dni_Con_Letra_Incorrecta_Es_Invalido()
    {
        _validator.Validate("12345678A").IsValid.Should().BeFalse();
    }

    [Test]
    public void Validate_Dni_Con_Formato_Incorrecto_Es_Invalido()
    {
        _validator.Validate("1234567").IsValid.Should().BeFalse();
        _validator.Validate("123456789Z").IsValid.Should().BeFalse();
        _validator.Validate("ABCDEFGHI").IsValid.Should().BeFalse();
    }

    [Test]
    public void Validate_Dni_Vacio_Es_Invalido()
    {
        _validator.Validate("").IsValid.Should().BeFalse();
        _validator.Validate("   ").IsValid.Should().BeFalse();
    }
}
