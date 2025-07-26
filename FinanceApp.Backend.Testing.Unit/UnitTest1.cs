/*using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.UserApi.UserCommands.CreateUser;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class CreateUserCommandHandlerTests
{
  [Fact]
  public async Task Test_CreateUser_Success()
  {
    // Arrange
    var loggerMock = new Mock<ILogger<CreateUserCommandHandler>>();

    var mapper = new MapperConfiguration(cfg =>
    {
      cfg.CreateMap<CreateUserCommand, User>();
    }).CreateMapper();

    var userRepositoryMock = new Mock<IRepository<User>>();
    var transactionGroupRepositoryMock = new Mock<ITransactionGroupRepository>();
    var unitOfWorkMock = new Mock<IUnitOfWork>();
    var smtpEmailSenderMock = new Mock<ISmtpEmailSender>();
    var bcryptServiceMock = new Mock<IBcryptService>();
    var jwtServiceMock = new Mock<IJwtService>();
    var cacheManagerMock = new Mock<ICacheManager>();

    // ...mock other dependencies as needed

    var handler = new CreateUserCommandHandler(
        loggerMock.Object,
        mapper,
        userRepositoryMock.Object,
        transactionGroupRepositoryMock.Object,
        unitOfWorkMock.Object,
        smtpEmailSenderMock.Object,
        bcryptServiceMock.Object,
        jwtServiceMock.Object,
        cacheManagerMock.Object
    );

    var createUserDto = new CreateUserDto
    {
      UserName = "testuser",
      Email = "test@example.com",
      Password = "TestPassword123!",
      BaseCurrency = CurrencyEnum.USD
    };

    var command = new CreateUserCommand(createUserDto, CancellationToken.None);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
  }
}

*/
