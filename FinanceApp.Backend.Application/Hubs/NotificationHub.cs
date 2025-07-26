using System.Security.Claims;
using FinanceApp.Backend.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.Hubs;

[Authorize]
public class NotificationHub : Hub
{
  private readonly ILogger<NotificationHub> _logger;

  public NotificationHub(ILogger<NotificationHub> logger)
  {
    _logger = logger;
  }

  public override async Task OnConnectedAsync()
  {
    try
    {
      var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      if (userId == null)
      {
        Context.Abort();
        return;
      }

      await base.OnConnectedAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error occurred during SignalR connection for ConnectionId: {ConnectionId}", Context.ConnectionId);
      throw new SignalRException("ON_CONNECTED", Context.ConnectionId, "Failed to establish SignalR connection.", ex);
    }
  }

  public async Task JoinGroup(string userId)
  {
    try
    {
      await Groups.AddToGroupAsync(Context.ConnectionId, userId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error joining group for ConnectionId: {ConnectionId}, UserId: {UserId}", Context.ConnectionId, userId);
      throw new SignalRException("JOIN_GROUP", Context.ConnectionId, userId, userId, "Failed to join SignalR group.", ex);
    }
  }

  public async Task LeaveGroup(string userId)
  {
    try
    {
      await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error leaving group for ConnectionId: {ConnectionId}, UserId: {UserId}", Context.ConnectionId, userId);
      throw new SignalRException("LEAVE_GROUP", Context.ConnectionId, userId, userId, "Failed to leave SignalR group.", ex);
    }
  }
}
