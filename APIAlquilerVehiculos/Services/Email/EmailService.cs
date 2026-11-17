using APIAlquilerVehiculos.Configuration;
using APIAlquilerVehiculos.Errors.Common;
using APIAlquilerVehiculos.Errors.Email;
using CSharpFunctionalExtensions;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace APIAlquilerVehiculos.Services.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<Result<bool, DomainError>> SendEmailAsync(string toEmail, string subject, string bodyHtml)
    {
        _logger.LogDebug("Preparando envío de correo a {ToEmail} | Asunto: '{Subject}'", toEmail, subject);

        if (string.IsNullOrWhiteSpace(toEmail) || !MailboxAddress.TryParse(toEmail, out var recipientAddress))
        {
            _logger.LogWarning("Envío de correo abortado: dirección inválida '{ToEmail}'", toEmail);
            return Result.Failure<bool, DomainError>(
                EmailErrors.SendFailed($"La dirección de correo '{toEmail}' no es válida.")
            );
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(recipientAddress);
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = bodyHtml };

            using var client = new SmtpClient();

            var secureSocketOptions = _emailSettings.EnableSsl
                ? SecureSocketOptions.Auto
                : SecureSocketOptions.None;

            _logger.LogDebug("Conectando a SMTP {Server}:{Port}", _emailSettings.Server, _emailSettings.Port);
            await client.ConnectAsync(_emailSettings.Server, _emailSettings.Port, secureSocketOptions);

            if (!string.IsNullOrEmpty(_emailSettings.UserName))
            {
                _logger.LogDebug("Autenticando usuario SMTP {UserName}", _emailSettings.UserName);
                await client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);
            }

            var response = await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation(
                "Correo enviado correctamente a {ToEmail} | Asunto: '{Subject}' | Respuesta: {SmtpResponse}",
                toEmail, subject, response);

            return Result.Success<bool, DomainError>(true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Envío de correo cancelado para {ToEmail} | Asunto: '{Subject}'", toEmail, subject);
            return Result.Failure<bool, DomainError>(
                EmailErrors.SendFailed("El envío del correo fue cancelado.")
            );
        }
        catch (AuthenticationException ex)
        {
            _logger.LogError(ex, "Error de autenticación SMTP con usuario {UserName} en {Server}",
                _emailSettings.UserName, _emailSettings.Server);
            return Result.Failure<bool, DomainError>(
                EmailErrors.SendFailed("Fallo de autenticación con el servidor de correo.")
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo a {ToEmail} | Asunto: '{Subject}'", toEmail, subject);
            return Result.Failure<bool, DomainError>(
                EmailErrors.SendFailed(ex.Message)
            );
        }
    }
}