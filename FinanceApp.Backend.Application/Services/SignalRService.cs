using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FinanceApp.Backend.Application.Services;

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
