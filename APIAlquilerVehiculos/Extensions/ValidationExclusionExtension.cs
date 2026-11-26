using APIAlquilerVehiculos.Dtos.Cientes;
using APIAlquilerVehiculos.Validators.Auth;
using APIAlquilerVehiculos.Validators.Auth.Documentos;
using APIAlquilerVehiculos.Validators.Clientes;
using FluentValidation;
using FluentValidation.AspNetCore;

public static class ValidationExclusionExtension
{
    public static void AddFluentAuto(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>(
        ServiceLifetime.Scoped,
        filter => filter.ValidatorType != typeof(PasaporteValidator)
            && filter.ValidatorType != typeof(NieValidator)
            && filter.ValidatorType != typeof(DniValidator));

        services.AddValidatorsFromAssemblyContaining<SignupDtoValidator>(
        ServiceLifetime.Scoped,
        filter => filter.ValidatorType != typeof(PasaporteValidator)
            && filter.ValidatorType != typeof(NieValidator)
            && filter.ValidatorType != typeof(DniValidator));
        services.AddValidatorsFromAssemblyContaining<UpdateClienteValidator>(
        ServiceLifetime.Scoped,
        filter => filter.ValidatorType != typeof(PasaporteValidator)
            && filter.ValidatorType != typeof(NieValidator)
            && filter.ValidatorType != typeof(DniValidator));

        services.AddFluentValidationAutoValidation();
    }
}