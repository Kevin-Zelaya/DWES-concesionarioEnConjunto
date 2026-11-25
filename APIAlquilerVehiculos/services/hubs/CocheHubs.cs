using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Serilog;

[Authorize]
public class CocheHubs : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = Context.User?.FindFirstValue(ClaimTypes.Email);
        var isAdmin = Context.User?.IsInRole("ADMIN") ?? false;

        // Añadimos al usuario a su grupo personal
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        // Añadimos al grupo de administradores
        if (isAdmin)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        }

        Log.Information("Usuario conectado: {Email} (ID: {UserId})", email, userId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        Log.Information("Usuario desconectado: {UserId}", userId);

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Unirse a un grupo de categoría.
    /// </summary>
    public async Task JoinCategoriaGroup(long categoriaId)
    {
        if (categoriaId <= 0)
            return;

        var groupName = $"categoria-{categoriaId}";

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        Log.Debug("Conexión {ConnectionId} unida al grupo {Group}", Context.ConnectionId, groupName);

        await Clients.Caller.SendAsync("JoinedGroup", groupName);
    }

    /// <summary>
    /// Salir de un grupo de categoría.
    /// </summary>
    public async Task LeaveCategoriaGroup(long categoriaId)
    {
        if (categoriaId <= 0)
            return;

        var groupName = $"categoria-{categoriaId}";

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        Log.Debug("Conexión {ConnectionId} eliminada del grupo {Group}", Context.ConnectionId, groupName);

        await Clients.Caller.SendAsync("LeftGroup", groupName);
    }
}