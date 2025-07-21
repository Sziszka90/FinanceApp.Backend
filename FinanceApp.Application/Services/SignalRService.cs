using Microsoft.AspNetCore.SignalR;
using FinanceApp.Application.Hubs;
using FinanceApp.Application.Abstraction.Services;

namespace FinanceApp.Application.Services;

public class SignalRService : ISignalRService
{
  private readonly IHubContext<NotificationHub> _hubContext;

  public SignalRService(IHubContext<NotificationHub> hubContext)
  {
    _hubContext = hubContext;
  }

  public async Task SendToClientGroupMethodAsync(string group, string clientMethod, string message)
  {
    await _hubContext.Clients.Group(group).SendAsync(clientMethod, message);
  }
}
