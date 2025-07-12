using System.Net;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Testing.Base;

namespace FinanceApp.Testing.TransactionGroupApi;

public class TransactionGroupApi : TestBase
{
  [Fact]
  public async Task DeleteTransactionGroup_ReturnsNothing()
  {
    // Arrange
    await InitializeAsync();
    var transactionGroup = await CreateTransactionGroupAsync();

    // Act
    await Client.DeleteAsync(TRANSACTION_GROUPS + transactionGroup!.Id);
    var response = await GetContentAsync<GetTransactionGroupDto>(await Client.GetAsync(TRANSACTION_GROUPS + transactionGroup!.Id));

    // Assert
    Assert.Null(response);
  }

  [Fact]
  public async Task DeleteNotExistingTransactionGroup_ReturnsNotFound()
  {
    // Arrange
    await InitializeAsync();
    var transactionGroup = await CreateTransactionGroupAsync();
    transactionGroup!.Id = Guid.NewGuid();

    // Act
    var response = await Client.DeleteAsync(TRANSACTION_GROUPS + transactionGroup!.Id);

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task GetAllTransactionGroup_ReturnsValidExpenseGroup()
  {
    // Arrange
    await InitializeAsync();
    var transactionGroup = await CreateTransactionGroupAsync();

    // Act
    var response = await GetContentAsync<List<GetTransactionGroupDto>>(await Client.GetAsync(TRANSACTION_GROUPS));

    // Assert
    Assert.Equal(transactionGroup!.Id, response![0].Id);
  }

  [Fact]
  public async Task GetTransactionGroupById_ReturnsValidExpenseGroup()
  {
    // Arrange
    await InitializeAsync();
    var transactionGroup = await CreateTransactionGroupAsync();

    // Act
    var response = await GetContentAsync<GetTransactionGroupDto>(await Client.GetAsync(TRANSACTION_GROUPS + transactionGroup!.Id));

    // Assert
    Assert.Equal(transactionGroup!.Id, response!.Id);
  }

  [Fact]
  public async Task UpdateTransactionGroup_ReturnsUpdatedExpenseGroup()
  {
    // Arrange
    await InitializeAsync();
    var transactionGroup = await CreateTransactionGroupAsync();

    var updatedTransactionGroup = new UpdateTransactionGroupDto
    {
      Id = transactionGroup!.Id,
      Name = "Updated Name",
      Description = "Updated Description",
      Limit = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = 200
      }
    };

    // Act
    await GetContentAsync<GetTransactionGroupDto>(await Client.PutAsync(TRANSACTION_GROUPS, CreateContent(updatedTransactionGroup)));
    var response = await GetContentAsync<GetTransactionGroupDto>(await Client.GetAsync(TRANSACTION_GROUPS + transactionGroup!.Id));

    // Assert
    Assert.Equal(transactionGroup!.Id, response!.Id);
    Assert.Equal(updatedTransactionGroup.Name, response.Name);
    Assert.Equal(updatedTransactionGroup.Limit.Amount, response.Limit!.Amount);
  }
}
