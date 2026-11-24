using FluentValidation;

/// <summary>
/// Valida los datos necesarios para crear un extra.
/// </summary>
public class ExtraCreateRequestValidator : AbstractValidator<ExtraCreateRequestDto>
{
    public ExtraCreateRequestValidator()
    {
        RuleFor(e => e.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(e => e.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(250).WithMessage("La descripción no puede superar los 250 caracteres.");

        RuleFor(e => e.Precio)
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.");

        RuleFor(e => e.TipoPrecio)
            .IsInEnum().WithMessage("El tipo de precio no es válido.");
    }
}

/// <summary>
/// Valida los datos necesarios para actualizar un extra.
/// </summary>
public class ExtraUpdateRequestValidator : AbstractValidator<ExtraUpdateRequestDto>
{
    public ExtraUpdateRequestValidator()
    {
        RuleFor(e => e.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(e => e.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(250).WithMessage("La descripción no puede superar los 250 caracteres.");

        RuleFor(e => e.Precio)
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.");

        RuleFor(e => e.TipoPrecio)
            .IsInEnum().WithMessage("El tipo de precio no es válido.");
    }
}