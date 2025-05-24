using System.Net;
using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Testing.Base;

namespace FinanceApp.Testing.ExpenseTransactionGroup;

public class ExpenseTransactionGroupApi : TestBase
{
  [Fact]
  public async Task DeleteExpenseGroup_ReturnsNothing()
  {
    // Arrange
    await InitializeAsync();
    var expenseGroup = await CreateExpenseTransactionGroupAsync();

    // Act
    await Client.DeleteAsync(EXPENSE_TRANSACTION_GROUPS + expenseGroup!.Id);
    var response = await GetContentAsync<GetExpenseTransactionGroupDto>(await Client.GetAsync(EXPENSE_TRANSACTION_GROUPS + expenseGroup!.Id));

    // Assert
    Assert.Null(response);
  }

  [Fact]
  public async Task DeleteNotExistingExpenseGroup_ReturnsNotFound()
  {
    // Arrange
    await InitializeAsync();
    var expenseGroup = await CreateExpenseTransactionGroupAsync();
    expenseGroup!.Id = Guid.NewGuid();

    // Act
    var response = await Client.DeleteAsync(EXPENSE_TRANSACTION_GROUPS + expenseGroup!.Id);

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task GetAllExpenseGroup_ReturnsValidExpenseGroup()
  {
    // Arrange
    await InitializeAsync();
    var expenseGroup = await CreateExpenseTransactionGroupAsync();

    // Act
    var response = await GetContentAsync<List<GetExpenseTransactionGroupDto>>(await Client.GetAsync(EXPENSE_TRANSACTION_GROUPS));

    // Assert
    Assert.Equal(expenseGroup!.Id, response![0].Id);
  }

  [Fact]
  public async Task GetExpenseGroupById_ReturnsValidExpenseGroup()
  {
    // Arrange
    await InitializeAsync();
    var expenseGroup = await CreateExpenseTransactionGroupAsync();

    // Act
    var response = await GetContentAsync<GetExpenseTransactionGroupDto>(await Client.GetAsync(EXPENSE_TRANSACTION_GROUPS + expenseGroup!.Id));

    // Assert
    Assert.Equal(expenseGroup!.Id, response!.Id);
  }

  [Fact]
  public async Task UpdateExpenseGroup_ReturnsUpdatedExpenseGroup()
  {
    // Arrange
    await InitializeAsync();
    var expenseGroup = await CreateExpenseTransactionGroupAsync();
    var updatedExpenseGroup = new UpdateExpenseTransactionGroupDto
    {
      Id = expenseGroup!.Id,
      Name = "Updated Name",
      Description = "Updated Description",
      Icon = expenseGroup.Icon,
      Limit = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = 200
      }
    };

    // Act
    await GetContentAsync<GetExpenseTransactionGroupDto>(await Client.PutAsync(EXPENSE_TRANSACTION_GROUPS, CreateContent(updatedExpenseGroup)));
    var response = await GetContentAsync<GetExpenseTransactionGroupDto>(await Client.GetAsync(EXPENSE_TRANSACTION_GROUPS + expenseGroup!.Id));

    // Assert
    Assert.Equal(expenseGroup!.Id, response!.Id);
    Assert.Equal(updatedExpenseGroup.Name, response.Name);
    Assert.Equal(updatedExpenseGroup.Limit.Amount, response.Limit!.Amount);
  }
}
