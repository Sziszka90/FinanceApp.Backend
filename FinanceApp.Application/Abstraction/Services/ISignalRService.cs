namespace   FinanceApp.Application.Abstraction.Services;

public interface ISignalRService
{
  /// <summary>
  /// Sends a message to a specific client group.
  /// </summary>
  /// <param name="group">The group name.</param>
  /// <param name="clientMethod">The client method to invoke.</param>
  /// <param name="message">The message to send.</param>
  Task SendToClientGroupMethodAsync(string group, string clientMethod, string message);
}
