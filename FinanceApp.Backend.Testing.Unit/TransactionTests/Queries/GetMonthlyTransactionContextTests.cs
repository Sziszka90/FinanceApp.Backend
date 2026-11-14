using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetMonthlyTransactionContext;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Queries;

public class GetMonthlyTransactionContextTests : TestBase
{
  [Fact]
  public async Task Handle_UsesAuthenticatedUserAndReturnsMonthlyTransactions()
  {
    var user = new User("Test User", "test@example.com", "hashed-password", CurrencyEnum.USD);
    var postedAt = new DateTimeOffset(2026, 12, 3, 10, 0, 0, TimeSpan.Zero);
    var transaction = new Transaction(
      "Salary",
      "Monthly salary",
      TransactionTypeEnum.Income,
      new Money { Amount = 5000m, Currency = CurrencyEnum.USD },
      5000m,
      null,
      postedAt,
      user);
    var logger = CreateLoggerMock<GetMonthlyTransactionContextQueryHandler>();
    UserServiceMock.Setup(service => service.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(user));
    TransactionRepositoryMock.Setup(repository => repository.GetTransactionsByDateRangeAsync(
        user.Id,
        It.IsAny<DateTimeOffset>(),
        It.IsAny<DateTimeOffset>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync([transaction]);

    var handler = new GetMonthlyTransactionContextQueryHandler(
      logger.Object,
      TransactionRepositoryMock.Object,
      UserServiceMock.Object,
      ExchangeRateServiceMock.Object);

    var result = await handler.Handle(new GetMonthlyTransactionContextQuery(2026, 12), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(2026, result.Data.SelectedYear);
    Assert.Equal(12, result.Data.SelectedMonth);
    Assert.Equal(6, result.Data.LookbackMonths);
    var item = Assert.Single(result.Data.Transactions);
    Assert.Equal("Salary", item.Name);
    Assert.True(item.IsInSelectedMonth);
    Assert.Equal(5000m, item.Amount);
    Assert.Equal("USD", item.Currency);
    ExchangeRateServiceMock.Verify(service => service.ConvertAmountAsync(
      It.IsAny<decimal>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_WhenAuthenticatedUserCannotBeResolved_ReturnsFailure()
  {
    var error = ApplicationError.UserNotLoggedInError();
    UserServiceMock.Setup(service => service.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Failure<User>(error));
    var handler = new GetMonthlyTransactionContextQueryHandler(
      CreateLoggerMock<GetMonthlyTransactionContextQueryHandler>().Object,
      TransactionRepositoryMock.Object,
      UserServiceMock.Object,
      ExchangeRateServiceMock.Object);

    var result = await handler.Handle(new GetMonthlyTransactionContextQuery(2026, 12), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Same(error, result.ApplicationError);
    TransactionRepositoryMock.Verify(repository => repository.GetTransactionsByDateRangeAsync(
      It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
  }
}
