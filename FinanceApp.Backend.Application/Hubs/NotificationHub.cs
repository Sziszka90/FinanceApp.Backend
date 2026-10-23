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

  public async Task JoinGroup(string groupId)
  {
    try
    {
      await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error joining group for ConnectionId: {ConnectionId}, GroupId: {GroupId}", Context.ConnectionId, groupId);
      throw new SignalRException("JOIN_GROUP", Context.ConnectionId, groupId, groupId, "Failed to join SignalR group.", ex);
    }
  }

  public async Task LeaveGroup(string groupId)
  {
    try
    {
      await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error leaving group for ConnectionId: {ConnectionId}, GroupId: {GroupId}", Context.ConnectionId, groupId);
      throw new SignalRException("LEAVE_GROUP", Context.ConnectionId, groupId, groupId, "Failed to leave SignalR group.", ex);
    }
  }
}
