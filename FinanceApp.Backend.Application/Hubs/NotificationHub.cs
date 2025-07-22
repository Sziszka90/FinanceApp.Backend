using Microsoft.AspNetCore.SignalR;

namespace FinanceApp.Backend.Application.Hubs;

public class NotificationHub : Hub
{
  public async Task JoinGroup(string userId)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, userId);
  }

  public async Task LeaveGroup(string userId)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
  }
}
