using APIAlquilerVehiculos.Dtos.Trabajadores;
using APIAlquilerVehiculos.Validators.Auth.Documentos;
using FluentValidation;

namespace APIAlquilerVehiculos.Validators.Trabajadores;

public class CreateTrabajadorDtoValidator : AbstractValidator<CreateTrabajadorDto> {
    
    public CreateTrabajadorDtoValidator() {
        
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);
        
        RuleFor(x => x.Telefono)
            .NotEmpty()
            .MaximumLength(20);
        
        RuleFor(x => x.Contraseña)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);
        
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);
        
        RuleFor(x => x.Apellidos)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(150);

        RuleFor(x => x.Dni!) 
            .SetValidator(new DniValidator()) 
            .When(x => !string.IsNullOrWhiteSpace(x.Dni));

        // la fecha es opcional, el mapper pone hoy si no hay
        RuleFor(x => x.FechaIngreso)
            .Must(f => f is null || f.Value.Date <= DateTime.UtcNow.Date)
            .WithMessage("La fecha de ingreso no puede ser futura");

        RuleFor(x => x.TipoTrabajador)
            .IsInEnum()
            .WithMessage("El tipo de trabajador debe ser Operaciones o Mecanico");
    }
}