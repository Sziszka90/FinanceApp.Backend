using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FinanceApp.Backend.Application.Hubs;

[Authorize]
public class NotificationHub : Hub
{
  public override async Task OnConnectedAsync()
  {
    var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userId == null)
    {
        Context.Abort();
        return;
    }

    await base.OnConnectedAsync();
  }

  public async Task JoinGroup(string userId)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, userId);
  }

  public async Task LeaveGroup(string userId)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
  }
}
