/// <summary>
/// Representa una notificación sobre un recurso del sistema.
/// </summary>
/// <typeparam name="T">Tipo del identificador del recurso.</typeparam>
public class ResourceNotification<T>
{
    public required string Resource { get; set; }
    public required string Action { get; set; }
    public required T ResourceId { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}