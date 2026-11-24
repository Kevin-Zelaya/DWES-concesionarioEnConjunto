using FluentValidation;

/// <summary>
/// Valida los datos necesarios para crear una póliza.
/// </summary>
public class PolizaCreateRequestValidator : AbstractValidator<PolizaCreateRequestDto>
{
    public PolizaCreateRequestValidator()
    {
        RuleFor(p => p.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(p => p.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(250).WithMessage("La descripción no puede superar los 250 caracteres.");

        RuleFor(p => p.IncrementoDiario)
            .GreaterThanOrEqualTo(0).WithMessage("El incremento diario no puede ser negativo.");
    }
}

/// <summary>
/// Valida los datos necesarios para actualizar una póliza.
/// </summary>
public class PolizaUpdateRequestValidator : AbstractValidator<PolizaUpdateRequestDto>
{
    public PolizaUpdateRequestValidator()
    {
        RuleFor(p => p.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(p => p.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(250).WithMessage("La descripción no puede superar los 250 caracteres.");

        RuleFor(p => p.IncrementoDiario)
            .GreaterThanOrEqualTo(0).WithMessage("El incremento diario no puede ser negativo.");
    }
}