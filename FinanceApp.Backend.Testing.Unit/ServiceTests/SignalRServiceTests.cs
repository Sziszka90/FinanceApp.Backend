using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Application.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.ServiceTests;

public class SignalRServiceTests
{
  [Fact]
  public async Task SendToClientGroupMethodAsync_CallsHubContext()
  {
    // arrange
    var hubContextMock = new Mock<IHubContext<NotificationHub>>();
    var clientsMock = new Mock<IHubClients>();
    var groupMock = new Mock<IClientProxy>();
    hubContextMock.Setup(x => x.Clients).Returns(clientsMock.Object);
    clientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(groupMock.Object);

    bool sendCalled = false;
    string? sentMethod = null;
    object[]? sentArgs = null;
    var proxy = groupMock.As<IClientProxy>();
    proxy.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
      .Callback<string, object[], CancellationToken>((method, args, token) =>
      {
        sendCalled = true;
        sentMethod = method;
        sentArgs = args;
      })
      .Returns(Task.CompletedTask);

    var service = new SignalRService(hubContextMock.Object);

    // act
    await service.SendToClientGroupMethodAsync("group1", "method", "message");

    // assert
    clientsMock.Verify(x => x.Group("group1"), Times.Once);
    Assert.True(sendCalled);
    Assert.Equal("method", sentMethod);
    Assert.NotNull(sentArgs);
    Assert.Single(sentArgs);
    Assert.Equal("message", sentArgs[0]);
  }
}
