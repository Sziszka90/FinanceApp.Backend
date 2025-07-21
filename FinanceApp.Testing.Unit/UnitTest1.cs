// using AutoMapper;
// using FinanceApp.Application.Abstraction.Clients;
// using FinanceApp.Application.Abstraction.Repositories;
// using FinanceApp.Application.Abstraction.Services;
// using FinanceApp.Application.UserApi.UserCommands.CreateUser;
// using FinanceApp.Domain.Entities;
// using Microsoft.Extensions.Logging;
// using Moq;
// using Xunit;

// public class CreateUserCommandHandlerTests
// {
//   [Fact]
//   public async Task Test_CreateUser_Success()
//   {
//     // Arrange
//     var loggerMock = new Mock<ILogger<CreateUserCommandHandler>>();

//     var mapper = new MapperConfiguration(cfg =>
//     {
//         cfg.CreateMap<CreateUserCommand, User>();
//     }).CreateMapper();

//     var userRepositoryMock = new Mock<IUserRepository>();
//     var unitOfWorkMock = new Mock<IUnitOfWork>();
//     var smtpEmailSenderMock = new Mock<ISmtpEmailSender>();
//     var jwtServiceMock = new Mock<IJwtService>();

//     // ...mock other dependencies as needed

//     var handler = new CreateUserCommandHandler(
//         loggerMock.Object,
//         userRepositoryMock.Object,
//         unitOfWorkMock.Object,
//         smtpEmailSenderMock.Object,
//         jwtServiceMock.Object
//         // ...other dependencies
//     );

//     var command = new CreateUserCommand { /* set properties */ };

//     // Act
//     var result = await handler.Handle(command, CancellationToken.None);

//     // Assert
//         smtpEmailSenderMock.Object
//     // ...other dependencies
//     );

//     var command = new CreateUserCommand { /* set properties */ };

//     // Act
//     var result = await handler.Handle(command, CancellationToken.None);

// // Assert
// Assert.True(result.IsSuccess);
//   }
// }

