using FluentValidation;

/// <summary>
/// Valida los datos necesarios para crear un recargo.
/// </summary>
public class RecargoCreateRequestValidator : AbstractValidator<RecargoCreateRequestDto>
{
    public RecargoCreateRequestValidator()
    {
        RuleFor(r => r.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(r => r.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(250).WithMessage("La descripción no puede superar los 250 caracteres.");

        RuleFor(r => r.TipoPrecio)
            .IsInEnum().WithMessage("El tipo de precio no es válido.");

        RuleFor(r => r.Importe)
            .GreaterThanOrEqualTo(0).WithMessage("El importe no puede ser negativo.");
    }
}

/// <summary>
/// Valida los datos necesarios para actualizar un recargo.
/// </summary>
public class RecargoUpdateRequestValidator : AbstractValidator<RecargoUpdateRequestDto>
{
    public RecargoUpdateRequestValidator()
    {
        RuleFor(r => r.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(r => r.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(250).WithMessage("La descripción no puede superar los 250 caracteres.");

        RuleFor(r => r.TipoPrecio)
            .IsInEnum().WithMessage("El tipo de precio no es válido.");

        RuleFor(r => r.Importe)
            .GreaterThanOrEqualTo(0).WithMessage("El importe no puede ser negativo.");
    }
}