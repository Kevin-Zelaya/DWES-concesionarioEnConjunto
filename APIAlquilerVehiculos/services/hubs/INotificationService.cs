public interface INotificationService
{
    /// <summary>
    /// Notifica a todos los administradores.
    /// </summary>
    Task NotifyAdminsAsync<T>(ResourceNotification<T> notification);

    /// <summary>
    /// Notifica a un usuario concreto.
    /// </summary>
    Task NotifyUserAsync<T>(string userId, ResourceNotification<T> notification);

    /// <summary>
    /// Notifica a todos los usuarios de un grupo.
    /// </summary>
    Task NotifyGroupAsync<T>(string groupName, ResourceNotification<T> notification);
}