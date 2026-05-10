using Microsoft.AspNetCore.SignalR;

namespace wdb_backend.Notification;

public class NotificationsHub : Hub
{
    // when the client connects
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"[Hub] client connect: {Context.ConnectionId}");

        // for sending the notification to the specific client, group workerId
        var workerId = Context.GetHttpContext()?.Request.Query["workerId"].ToString();
        if (! string.IsNullOrEmpty(workerId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, workerId);
        }

        await base.OnConnectedAsync();
    }

    // when the client disconnects
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"[Hub] client closed: {Context.ConnectionId}");

        // remove the group
        var workerId = Context.GetHttpContext()?.Request.Query["workerId"].ToString();
        if (!string.IsNullOrEmpty(workerId))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, workerId);

        await base.OnDisconnectedAsync(exception);
    }
}
