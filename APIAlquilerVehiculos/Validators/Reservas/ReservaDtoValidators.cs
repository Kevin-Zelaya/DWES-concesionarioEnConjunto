
using FluentValidation;

public class CreateReservaRequestDtoValidator : AbstractValidator<ReservaCreateRequestDto>
{
    public CreateReservaRequestDtoValidator()
    {
        RuleFor(x => x.ClienteId)
            .GreaterThan(0)
            .WithMessage("El cliente es obligatorio.");

        RuleFor(x => x.Categoria)
            .IsInEnum()
            .WithMessage("La categoría seleccionada no es válida.");

        RuleFor(x => x.VehiculoId)
            .NotEmpty()
            .WithMessage("El vehículo es obligatorio.");

        RuleFor(x => x.FechaRecogida)
            .NotEmpty()
            .WithMessage("La fecha de recogida es obligatoria.");

        RuleFor(x => x.FechaDevolucion)
            .NotEmpty()
            .Must((reserva, fechaDevolucion) =>
                fechaDevolucion - reserva.FechaRecogida == TimeSpan.FromHours(24))
            .WithMessage("La fecha de devolución debe ser exactamente 24 horas posterior a la fecha de recogida.");

        RuleFor(x => x.Lineas)
            .NotNull()
            .NotEmpty()
            .WithMessage("La reserva debe contener al menos una línea.");
    }
}


public class ReservaUpdateFechaRequestDtoValidator : AbstractValidator<ReservaUpdateFechaRequestDto>
{
    public ReservaUpdateFechaRequestDtoValidator()
    {
        RuleFor(x => x.FechaRecogida)
            .NotEmpty()
            .WithMessage("La fecha de recogida es obligatoria.");

        RuleFor(x => x.FechaDevolucion)
            .NotEmpty()
            .Must((reserva, fechaDevolucion) =>
                fechaDevolucion - reserva.FechaRecogida == TimeSpan.FromHours(24))
            .WithMessage("La fecha de devolución debe ser exactamente 24 horas posterior a la fecha de recogida.");

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage("La versión del registro es obligatoria.");
    }
}

