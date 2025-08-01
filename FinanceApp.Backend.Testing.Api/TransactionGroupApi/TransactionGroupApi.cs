using System.Net;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Testing.Api.Base;

namespace FinanceApp.Backend.Testing.Api.TransactionGroupApi;

public class TransactionGroupApi : TestBase
{
  [Fact]
  public async Task DeleteTransactionGroup_ReturnsNothing()
  {
    // arrange
    await InitializeAsync();
    var transactionGroup = await CreateTransactionGroupAsync();

    // act
    await Client.DeleteAsync(TRANSACTION_GROUPS + transactionGroup!.Id);
    var response = await GetContentAsync<GetTransactionGroupDto>(await Client.GetAsync(TRANSACTION_GROUPS + transactionGroup!.Id));

    // assert
    Assert.Equal(Guid.Empty, response!.Id);
  }

  [Fact]
  public async Task DeleteNotExistingTransactionGroup_ReturnsNotFound()
  {
    // arrange
    await InitializeAsync();
    var transactionGroup = await CreateTransactionGroupAsync();
    transactionGroup!.Id = Guid.NewGuid();

    // act
    var response = await Client.DeleteAsync(TRANSACTION_GROUPS + transactionGroup!.Id);

    // assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task GetAllTransactionGroup_ReturnsValidExpenseGroup()
  {
    // arrange
    await InitializeAsync();
    var transactionGroup = await CreateTransactionGroupAsync();

    // act
    var response = await GetContentAsync<List<GetTransactionGroupDto>>(await Client.GetAsync(TRANSACTION_GROUPS));

    // assert
    Assert.Equal(transactionGroup!.Id, response![17].Id);
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
    // arrange
    await InitializeAsync();
    var transactionGroup = await CreateTransactionGroupAsync();

    var updatedTransactionGroup = new UpdateTransactionGroupDto
    {
      Id = transactionGroup!.Id,
      Name = "Updated Name",
      Description = "Updated Description"
    };

    // act
    await GetContentAsync<GetTransactionGroupDto>(await Client.PutAsync(TRANSACTION_GROUPS + transactionGroup!.Id, CreateContent(updatedTransactionGroup)));
    var response = await GetContentAsync<GetTransactionGroupDto>(await Client.GetAsync(TRANSACTION_GROUPS + transactionGroup!.Id));

    // assert
    Assert.Equal(transactionGroup!.Id, response!.Id);
    Assert.Equal(updatedTransactionGroup.Name, response.Name);
    Assert.Equal(updatedTransactionGroup.Description, response.Description);
  }
}
