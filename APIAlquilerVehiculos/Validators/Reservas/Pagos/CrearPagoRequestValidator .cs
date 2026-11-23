using APIAlquilerVehiculos.DTOs.Pagos;
using FluentValidation;

namespace APIAlquilerVehiculos.Validators.Pagos;

public class CrearPagoRequestValidator : AbstractValidator<CrearPagoRequestDto>
{
    public CrearPagoRequestValidator()
    {

        RuleFor(p => p.NumeroTarjeta)
            .NotEmpty()
            .WithMessage("El número de tarjeta es obligatorio.")
            .Must(EsNumeroValido)
            .WithMessage("El número de tarjeta no es válido.");
    }

    private static bool EsNumeroValido(string? numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            return false;

        numero = numero.Replace(" ", "");

        if (!numero.All(char.IsDigit))
            return false;

        int suma = 0;
        bool duplicar = false;

        for (int i = numero.Length - 1; i >= 0; i--)
        {
            int digito = numero[i] - '0';

            if (duplicar)
            {
                digito *= 2;

                if (digito > 9)
                    digito -= 9;
            }

            suma += digito;
            duplicar = !duplicar;
        }

        return suma % 10 == 0;
    }
}