using APIAlquilerVehiculos.Dtos.Auth;
using APIAlquilerVehiculos.Validators.Auth.Documentos;
using FluentValidation;

namespace APIAlquilerVehiculos.Validators.Auth;

public class SignupDtoValidator : AbstractValidator<SignupDto> {

    public SignupDtoValidator() {
        
        // contraseña fuerte
        RuleFor(x => x.Contraseña) 
            .Must(c => c.Any(char.IsUpper))
            .WithMessage("La contraseña debe contener al menos una mayúscula.") 
            .Must(c => c.Any(char.IsLower))
            .WithMessage("La contraseña debe contener al menos una minúscula.") 
            .Must(c => c.Any(char.IsDigit))
            .WithMessage("La contraseña debe contener al menos un número.");
        
        // no valida mayoría de edad, se hace en reserva
        RuleFor(x => x.FechaNacimiento)
            .Must(f => f != default)
            .WithMessage("La fecha de nacimiento es obligatoria.") 
            .Must(f => f.Date < DateTime.UtcNow.Date)
            .WithMessage("La fecha de nacimiento debe ser anterior a hoy.") 
            .GreaterThan(new DateTime(1900, 1, 1))
            .WithMessage("La fecha de nacimiento no es válida.");
        
        // validacion de documentos solo si tienen valor
        RuleFor(x => x.Dni!) 
            .SetValidator(new DniValidator()) 
            .When(x => !string.IsNullOrWhiteSpace(x.Dni));
        RuleFor(x => x.Nie!) 
            .SetValidator(new NieValidator())
            .When(x => !string.IsNullOrWhiteSpace(x.Nie)); 
        RuleFor(x => x.Pasaporte!)
            .SetValidator(new PasaporteValidator()) 
            .When(x => !string.IsNullOrWhiteSpace(x.Pasaporte));
        
        // exige al menos un documento
        RuleFor(x => x) 
            .Must(x => !string.IsNullOrWhiteSpace(x.Dni) 
                       || !string.IsNullOrWhiteSpace(x.Nie) 
                       || !string.IsNullOrWhiteSpace(x.Pasaporte)) 
            .WithName("Documentos") 
            .WithMessage("Debe aportar al menos un documento de identidad válido (DNI, NIE o Pasaporte).");
        
        // cuando solo se pasa el pasaporte se exige el nº de permido de conducir
        RuleFor(x => x.NumeroPermisoConducir) 
            .NotEmpty() 
            .WithMessage("Si se registra únicamente con pasaporte, debe indicar el número del permiso de conducir.") 
            .When(x => string.IsNullOrWhiteSpace(x.Dni) 
                       && string.IsNullOrWhiteSpace(x.Nie) 
                       && !string.IsNullOrWhiteSpace(x.Pasaporte));
    }
}