using System.Diagnostics;
using System.Text.RegularExpressions;
using FluentValidation;

namespace APIAlquilerVehiculos.Validators.Auth.Documentos;

/// <summary>
/// Validador para el NIE.
/// </summary>
public partial class NieValidator : AbstractValidator<string> {
    
    // para el cálculo del dígito de control
    private const string Letras = "TRWAGMYFPDXBNJZSQVHLCKE";
    
    // regex (8 digitos + una letra)
    [GeneratedRegex(@"^[XYZxyz]\d{7}[A-Za-z]$")] 
    private static partial Regex NieRegex();
    
    // constructor
    public NieValidator() {
        RuleFor(nie => nie) 
            .Must(EsNieValido) 
            .WithMessage("El NIE no es válido. Debe empezar por X, Y o Z, seguido de 7 dígitos y su letra de control."); 
    }

    private static bool EsNieValido(string? nie) {
        
        // si está vacio
        if (string.IsNullOrWhiteSpace(nie)) 
            return false; 
        
        // se limpia el string
        var limpio = nie.Trim().ToUpperInvariant(); 
        
        // si cumple el regex
        if (!NieRegex().IsMatch(limpio)) 
            return false; 
        
        var prefijo = limpio[0] 
            switch {
                'X' => '0', 
                'Y' => '1',
                'Z' => '2',
                _ => throw new UnreachableException() // no tiene un digito esperado
            }; 
        
        // se comprueba que la letra sea la deseada
        var numero = int.Parse($"{prefijo}{limpio[1..8]}"); 
        var letraEsperada = Letras[numero % 23]; 
        return limpio[8] == letraEsperada;
    }
}