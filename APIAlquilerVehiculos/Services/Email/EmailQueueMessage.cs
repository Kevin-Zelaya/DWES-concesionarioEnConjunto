namespace APIAlquilerVehiculos.Services.Email;

public record EmailQueueMessage(
    string ToEmail,
    string Subject,
    string BodyHtml,
    int Intentos = 0
);