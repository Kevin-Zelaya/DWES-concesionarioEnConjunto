using FluentValidation;

namespace APIAlquilerVehiculos.Validators.Auth.Documentos;

/// <summary>
/// Validador para el Pasaporte.
/// </summary>
public class PasaporteValidator : AbstractValidator<string> {
    
    private static readonly int[] Pesos = [7, 3, 1];

    public PasaporteValidator() {
        RuleFor(pasaporte => pasaporte) 
            .Must(EsPasaporteMrzValido) 
            .WithMessage("El pasaporte no es válido. Debe incluir los 9 caracteres del documento seguidos de su dígito de control MRZ (10 caracteres en total).");
    }

    public static bool EsPasaporteMrzValido(string? pasaporte) {
        
        // si está vacio
        if (string.IsNullOrWhiteSpace(pasaporte)) return false;
        
        // se limpia
        var limpio = pasaporte.Trim().ToUpperInvariant();
        
        // si no tienes los digitos necesarios
        if (limpio.Length != 10) return false;
        
        var cuerpo = limpio[..9]; 
        var controlChar = limpio[^1]; 
        
        if (!char.IsAsciiDigit(controlChar)) 
            return false; 
        
        var digitoEsperado = CalcularDigitoControl(cuerpo); 
        if (digitoEsperado < 0) 
            return false; 
        return digitoEsperado == (controlChar - '0');
    }

    public static int CalcularDigitoControl(string cadena) {
        
        if (string.IsNullOrEmpty(cadena)) 
            return -1; var suma = 0;
        
        for (var i = 0; i < cadena.Length; i++) {
            var valor = ObtenerValorCaracter(cadena[i]); 
            if (valor < 0) return -1; 
            suma += valor * Pesos[i % 3];
        } 
        return suma % 10;
    } 
    
    private static int ObtenerValorCaracter(char c) => c switch {
        >= '0' and <= '9' => c - '0',
        >= 'A' and <= 'Z' => c - 'A' + 10, 
        '<' => 0, _ => -1
    };
}