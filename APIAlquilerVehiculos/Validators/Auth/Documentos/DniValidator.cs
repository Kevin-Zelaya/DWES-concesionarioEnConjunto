using System.Text.RegularExpressions;
using FluentValidation;

namespace APIAlquilerVehiculos.Validators.Auth.Documentos;

/// <summary>
/// Validador para un DNI.
/// </summary>
public partial class DniValidator : AbstractValidator<string> {
    
    // para el cálculo del dígito de control
    private const string Letras = "TRWAGMYFPDXBNJZSQVHLCKE";
    
    // regex (8 digitos + una letra)
    [GeneratedRegex(@"^\d{8}[A-Za-z]$")] 
    private static partial Regex DniRegex();
    
    // constructor
    public DniValidator() {
        RuleFor(dni => dni) 
        .Must(EsDniValido) 
        .WithMessage("El DNI no es válido. Debe tener 8 dígitos y la letra de control correcta."); 
    }

    private static bool EsDniValido(string? dni) {
        
        // si está vacio
        if (string.IsNullOrWhiteSpace(dni)) 
            return false; 
        
        // se limpia el string
        var limpio = dni.Trim().ToUpperInvariant(); 
        
        // si cumple el regex
        if (!DniRegex().IsMatch(limpio))
            return false; 
        
        // se comprueba que la letra sea la deseada
        var numero = int.Parse(limpio[..8]); 
        var letraEsperada = Letras[numero % 23]; 
        return limpio[8] == letraEsperada;
    }
}