using System.Net;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Testing.Base;

namespace FinanceApp.Testing.Transaction;

public class TransactionApi : TestBase
{
  [Fact]
  public async Task CreateTransaction_WhenTransactionGroupDoesNotExists_ReturnsNotExists()
  {
    // Arrange
    await InitializeAsync();
    var transactionContent = CreateContent(new CreateTransactionDto
    {
      Name = "TestTransaction",
      Description = "Test Transaction",
      Value = new Money
      {
        Amount = 100,
        Currency = CurrencyEnum.HUF
      },
      TransactionDate = new DateTimeOffset(),
      TransactionGroupId = new Guid().ToString()
    });

    // Act
    var response = await Client.PostAsync(TRANSACTIONS, transactionContent);
    var content = await response.Content.ReadAsStringAsync();

    // Assert
    Assert.Contains(ApplicationError.TRANSACTION_GROUP_NOT_EXISTS_CODE, content);
  }

  [Fact]
  public async Task DeleteTransaction_DoesNotDeleteTransactionGroup()
  {
    // Arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();
    await Client.DeleteAsync(TRANSACTIONS + transaction!.Id);

    // Act
    var response = await GetContentAsync<GetTransactionGroupDto>(await Client.GetAsync(TRANSACTION_GROUPS + transaction!.TransactionGroup!.Id));

    // Assert
    Assert.Equal(response!.Id, transaction.TransactionGroup!.Id);
  }

  [Fact]
  public async Task DeleteTransaction_ReturnsNothing()
  {
    // Arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();

    // Act
    await Client.DeleteAsync(TRANSACTIONS + transaction!.Id);
    var response = await GetContentAsync<GetTransactionDto>(await Client.GetAsync(TRANSACTIONS + transaction!.Id));

    // Assert
    Assert.Null(response);
  }

  [Fact]
  public async Task DeleteTransactionAndTransactionGroup_ReturnsNoContent()
  {
    // Arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();


    // Act

    var responseTransaction = await Client.DeleteAsync(TRANSACTIONS + transaction!.Id);
    var responseTransactionGroup = await Client.DeleteAsync(TRANSACTION_GROUPS + transaction!.TransactionGroup!.Id);

    // Assert
    Assert.Equal(HttpStatusCode.NoContent, responseTransaction.StatusCode);
    Assert.Equal(HttpStatusCode.NoContent, responseTransactionGroup.StatusCode);
  }

  [Fact]
  public async Task DeleteNotExistingTransaction_ReturnsNotFound()
  {
    // Arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();
    transaction!.Id = Guid.NewGuid();

    // Act
    var response = await Client.DeleteAsync(TRANSACTIONS + transaction!.Id);

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }


  [Fact]
  public async Task GetAllTransaction_ReturnsValidTransaction()
  {
    // Arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();

    // Act
    var response = await GetContentAsync<List<GetTransactionDto>>(await Client.GetAsync(TRANSACTIONS));

    // Assert
    Assert.Equal(transaction!.Id, response![0].Id);
  }

  [Fact]
  public async Task GetAllTransactionSummary_ReturnsAllTransactionSummary()
  {
    // Arrange
    await InitializeAsync();
    var transactions = await CreateMultipleTransactionAsync();

    // Act
    var response = await Client.GetAsync(TRANSACTIONS_SUMMARY);
    var sum = await GetContentAsync<Money>(response);

    // Assert
    Assert.True(sum!.Amount < transactions[0].Value.Amount);
  }

  [Fact]
  public async Task GetTransactionById_ReturnsValidTransaction()
  {
    // Arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();

    // Act
    var response = await GetContentAsync<GetTransactionDto>(await Client.GetAsync(TRANSACTIONS + transaction!.Id));

    // Assert
    Assert.Equal(transaction!.Id, response!.Id);
  }

  [Fact]
  public async Task UpdateTransaction_ReturnsUpdatedTransaction()
  {
    // Arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();
    var updatedTransaction = new UpdateTransactionDto
    {
      Id = transaction!.Id,
      Name = "Updated Name",
      Description = "Updated Description",
      Value = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = 200
      },
      TransactionDate = transaction.TransactionDate,
      TransactionGroupId = transaction.TransactionGroup!.Id
    };

    // Act
    await GetContentAsync<GetTransactionDto>(await Client.PutAsync(TRANSACTIONS, CreateContent(updatedTransaction)));
    var response = await GetContentAsync<GetTransactionDto>(await Client.GetAsync(TRANSACTIONS + transaction!.Id));

    // Assert
    Assert.Equal(transaction!.Id, response!.Id);
    Assert.Equal(updatedTransaction.Name, response.Name);
    Assert.Equal(updatedTransaction.Value.Amount, response.Value.Amount);
  }

  [Fact]
  public async Task UpdateTransactionNegativeValue_ReturnsValidationError()
  {
    // Arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();
    var updatedTransaction = new UpdateTransactionDto
    {
      Id = transaction!.Id,
      Name = "Updated Name",
      Description = "Updated Description",
      Value = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = -200
      },
      TransactionDate = transaction.TransactionDate,
      TransactionGroupId = transaction.TransactionGroup!.Id
    };

    // Act
    var response = await Client.PutAsync(TRANSACTIONS, CreateContent(updatedTransaction));
    var responseContentAsString = await response.Content.ReadAsStringAsync();

    // Assert
    Assert.Contains(ApplicationError.VALIDATION_MESSAGE, responseContentAsString);
  }
}
