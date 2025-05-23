using System.Net;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.ExpenseTransactionDtos;
using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Testing.Base;

namespace FinanceApp.Testing.ExpenseTransaction;

public class ExpenseTransactionApi : TestBase
{
  [Fact]
  public async Task CreateExpense_WhenTransactionGroupDoesNotExists_ReturnsFailure()
  {
    // Arrange
    await InitializeAsync();
    var expenseContent = CreateContent(new CreateExpenseTransactionDto
    {
      Name = "TestExpense",
      Description = "Test Expense",
      Value = new Money
      {
        Amount = 100,
        Currency = CurrencyEnum.HUF
      },
      DueDate = new DateTimeOffset(),
      TransactionGroupId = new Guid()
    });

    // Act
    var response = await Client.PostAsync(EXPENSE_TRANSACTIONS, expenseContent);
    var content = await response.Content.ReadAsStringAsync();

    // Assert
    Assert.Contains(ApplicationError.TRANSACTION_GROUP_NOT_EXISTS_CODE, content);
  }

  [Fact]
  public async Task DeleteExpense_DoesNotDeleteExpenseGroup()
  {
    // Arrange
    await InitializeAsync();
    var expense = await CreateExpenseAsync();
    await Client.DeleteAsync(EXPENSE_TRANSACTIONS + expense!.Id);

    // Act
    var response = await GetContentAsync<GetExpenseTransactionGroupDto>(await Client.GetAsync(EXPENSE_TRANSACTION_GROUPS + expense!.TransactionGroup!.Id));

    // Assert
    Assert.Equal(response!.Id, expense.TransactionGroup!.Id);
  }

  [Fact]
  public async Task DeleteExpenseAndExpenseGroup_ReturnsNoContent()
  {
    // Arrange
    await InitializeAsync();
    var expense = await CreateExpenseAsync();

    // Act
    var responseExpense = await Client.DeleteAsync(EXPENSE_TRANSACTIONS + expense!.Id);
    var responseExpenseGroup = await Client.DeleteAsync(EXPENSE_TRANSACTION_GROUPS + expense!.TransactionGroup!.Id);

    // Assert
    Assert.Equal(responseExpense.StatusCode, HttpStatusCode.NoContent);
    Assert.Equal(responseExpenseGroup.StatusCode, HttpStatusCode.NoContent);
  }

  [Fact]
  public async Task DeleteExpenseAndQuery_ReturnsNothing()
  {
    // Arrange
    await InitializeAsync();
    var expense = await CreateExpenseAsync();

    // Act
    await Client.DeleteAsync(EXPENSE_TRANSACTIONS + expense!.Id);
    var response = await GetContentAsync<GetExpenseTransactionDto>(await Client.GetAsync(EXPENSE_TRANSACTIONS + expense!.Id));

    // Assert
    Assert.Null(response);
  }

  [Fact]
  public async Task DeleteNotExistingExpense_ReturnsNotFound()
  {
    // Arrange
    await InitializeAsync();
    var expense = await CreateExpenseAsync();
    expense!.Id = Guid.NewGuid();

    // Act
    var response = await Client.DeleteAsync(EXPENSE_TRANSACTIONS + expense!.Id);

    // Assert
    Assert.Equal(response.StatusCode, HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetAllExpense_ReturnsValidExpense()
  {
    // Arrange
    await InitializeAsync();
    var expense = await CreateExpenseAsync();

    // Act
    var response = await GetContentAsync<List<GetExpenseTransactionDto>>(await Client.GetAsync(EXPENSE_TRANSACTIONS));

    // Assert
    Assert.Equal(expense!.Id, response![0].Id);
  }

  [Fact]
  public async Task GetAllExpenseSummary_ReturnsAllExpenseSummary()
  {
    // Arrange
    await InitializeAsync();
    var incomes = await CreateMultipleExpenseAsync();

    // Act
    var response = await Client.GetAsync(EXPENSE_TRANSACTIONS_SUMMARY);
    var sum = await GetContentAsync<Money>(response);

    // Assert
    Assert.True(sum!.Amount < incomes[0].Value.Amount);
  }

  [Fact]
  public async Task GetExpenseById_ReturnsValidExpense()
  {
    // Arrange
    await InitializeAsync();
    var expense = await CreateExpenseAsync();

    // Act
    var response = await GetContentAsync<GetExpenseTransactionDto>(await Client.GetAsync(EXPENSE_TRANSACTIONS + expense!.Id));

    // Assert
    Assert.Equal(expense!.Id, response!.Id);
  }

  [Fact]
  public async Task UpdateExpense_ReturnsUpdatedExpense()
  {
    // Arrange
    await InitializeAsync();
    var expense = await CreateExpenseAsync();
    var updatedExpense = new UpdateExpenseTransactionDto
    {
      Id = expense!.Id,
      Name = "Updated Name",
      Description = "Updated Description",
      Value = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = 200
      },
      DueDate = expense.DueDate,
      TransactionGroupId = expense.TransactionGroup!.Id
    };

    // Act
    await GetContentAsync<GetExpenseTransactionDto>(await Client.PutAsync(EXPENSE_TRANSACTIONS, CreateContent(updatedExpense)));
    var response = await GetContentAsync<GetExpenseTransactionDto>(await Client.GetAsync(EXPENSE_TRANSACTIONS + expense!.Id));

    // Assert
    Assert.Equal(expense!.Id, response!.Id);
    Assert.Equal(updatedExpense.Name, response.Name);
    Assert.Equal(updatedExpense.Value.Amount, response.Value.Amount);
  }

  [Fact]
  public async Task UpdateExpenseNegativeValue_ReturnsValidationError()
  {
    // Arrange
    await InitializeAsync();
    var expense = await CreateExpenseAsync();
    var updatedExpense = new UpdateExpenseTransactionDto
    {
      Id = expense!.Id,
      Name = "Updated Name",
      Description = "Updated Description",
      Value = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = -200
      },
      DueDate = expense.DueDate,
      TransactionGroupId = expense.TransactionGroup!.Id
    };

    // Act
    var response = await Client.PutAsync(EXPENSE_TRANSACTIONS, CreateContent(updatedExpense));
    var responseContentAsString = await response.Content.ReadAsStringAsync();

    // Assert
    Assert.Contains(ApplicationError.VALIDATION_MESSAGE, responseContentAsString);
  }
}