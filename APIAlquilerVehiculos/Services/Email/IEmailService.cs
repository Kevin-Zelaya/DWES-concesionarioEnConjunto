using APIAlquilerVehiculos.Errors.Common;
using CSharpFunctionalExtensions;

namespace APIAlquilerVehiculos.Services.Email;

public interface IEmailService
{
    Task<Result<bool, DomainError>> SendEmailAsync(string toEmail, string subject, string bodyHtml);
}