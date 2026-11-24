using Microsoft.AspNetCore.SignalR;
using Serilog;

public class NotificationService(IHubContext<CocheHubs> hubContext) : INotificationService
{
    public async Task NotifyAdminsAsync<T>(ResourceNotification<T> notification)
    {
        await NotifyGroupAsync("admins", notification);
    }

    public async Task NotifyUserAsync<T>(string userId, ResourceNotification<T> notification)
    {
        await NotifyGroupAsync($"user-{userId}", notification);
    }

    public async Task NotifyGroupAsync<T>(string groupName, ResourceNotification<T> notification)
    {
        try
        {
            await hubContext.Clients.Group(groupName).SendAsync("ResourceUpdated", notification);

            Log.Information("Notificación {Resource} - {Action} enviada al grupo {Group}", notification.Resource, notification.Action, groupName);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error al enviar notificación al grupo {Group}", groupName);
        }
    }
}