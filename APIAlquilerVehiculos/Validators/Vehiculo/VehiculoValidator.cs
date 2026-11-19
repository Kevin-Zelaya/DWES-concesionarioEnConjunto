using APIAlquilerVehiculos.Dtos.Vehiculo;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Models.Enums;
using CSharpFunctionalExtensions;
using FluentValidation;

namespace APIAlquilerVehiculos.Validators.Vehiculo;

public class VehiculoValidator : AbstractValidator<VehiculoCreateDto>
{
    public VehiculoValidator()
    {
        RuleFor(x => x.Acriss)
            .NotEmpty()
            .Length(4)
            .Matches(@"^[MNEHCDIJSRFGPULWOX]{1}[BCDWVLSTFJXPQZEMRHYNGK]{1}[MNCABDQHF]{1}[VZDQHIECRNLSABMFUX]{1}$")
            .WithMessage("El codigo acriss debe tener un formato valido");
        
        RuleFor(x => x.Autonomia)
            .NotEmpty()
            .WithMessage("El campo autonomia no debe ser nulo");

        RuleFor(x => x).Must(RelacionCategoriaAcris)
            .WithMessage("El codigo acris y la categoria deven estar correlacionados");
        
        RuleFor(x => x.Categoria)
            .NotEmpty()
            .Must(x => x is CategoriaEnum);
        
        RuleFor(x => x.Color).NotEmpty()
            .WithMessage("El campo color no debe ser nulo");
        
        RuleFor(x => x.Combustible).NotEmpty()
            .WithMessage("El campo combustible no debe ser nulo");
        
        RuleFor(x => x.ConcesionarioId).NotEmpty()
            .WithMessage("El campo concesionario no debe ser nulo");
        
        RuleFor(x => x.Maletero).NotEmpty()
            .WithMessage("El campo maletero no debe ser nulo");
        
        RuleFor(x => x.Matricula)
            .NotEmpty()
            .WithMessage("El campo matricula no deve ser nulo")
            .Matches(@"^[0-9]{4}[BCDFGHJKLMNPRSTVWXYZ]{3}$")
            .WithMessage("La matricula debe tener un formato valido");
        
        RuleFor(x => x.Modelo).NotEmpty()
            .WithMessage("El campo modelo no debe ser nulo");
        
        RuleFor(x => x.PrecioPorDia).NotEmpty()
            .WithMessage("El campo PrecioPorDia no debe ser nulo");
        
        RuleFor(x => x.NumeroPlazas).NotEmpty()
            .WithMessage("El campo numeroPlazas no debe ser nulo");
        
        RuleFor(x => x.Estado)
            .Equal(EstadoEnum.Disponible)
            .WithMessage("Un vehiculo creado debe estar disponible")
            .NotEmpty()
            .WithMessage("El campo Estado no debe ser nulo");
    }

    private static bool RelacionCategoriaAcris(VehiculoCreateDto obj)
    {
        var acris = obj.Acriss.Substring(2,1);
        var categoria = obj.Categoria;

        bool correct = acris switch
        {
            "B" => categoria==CategoriaEnum.Hatchback,
            "C" => categoria==CategoriaEnum.Hatchback,
            "D" => categoria==CategoriaEnum.Hatchback,
            "W" => categoria==CategoriaEnum.Familiar,
            "V" => categoria==CategoriaEnum.Furgoneta,
            "K" => categoria==CategoriaEnum.Furgoneta,
            "H" => categoria==CategoriaEnum.Furgoneta,
            "L" => categoria==CategoriaEnum.Sedan,
            "T" => categoria==CategoriaEnum.Descapotable,
            "N" => categoria==CategoriaEnum.Descapotable,
            "F" => categoria==CategoriaEnum.Sub,
            "G" => categoria==CategoriaEnum.Sub,
            "J" => categoria==CategoriaEnum.Todoterreno,
            "E" => categoria==CategoriaEnum.Coupe,
            "M" => categoria==CategoriaEnum.Monovolumen,
            _ => false
        };
        /*
         * Deportivo
         * S deportivo,
         *
         * Especial
         * X especial,
         * Z oferta especial,
         * R recreativo,
         *
         * PickUp
         * P pickup de cabina simple,
         * Q pickup de doble cabina,
         */
        return correct;
    }
}