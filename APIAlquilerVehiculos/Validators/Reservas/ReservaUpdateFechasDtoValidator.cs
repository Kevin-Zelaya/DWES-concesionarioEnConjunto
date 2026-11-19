using FluentValidation;

public class SignupDtoValidator : AbstractValidator<ReservaUpdateFechaRequestDto> {

    public SignupDtoValidator() {
        // proximamente
        
        // // contraseña fuerte
        // RuleFor(x => x.Contraseña) 
        //     .Must(c => c.Any(char.IsUpper))
        //     .WithMessage("La contraseña debe contener al menos una mayúscula.") 
        //     .Must(c => c.Any(char.IsLower))
        //     .WithMessage("La contraseña debe contener al menos una minúscula.") 
        //     .Must(c => c.Any(char.IsDigit))
        //     .WithMessage("La contraseña debe contener al menos un número.");
        
        
    }
}