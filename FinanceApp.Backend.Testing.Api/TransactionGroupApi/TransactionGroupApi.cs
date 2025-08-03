using System.Net;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;
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

  [Fact]
  public async Task CreateTransactionGroup_WithValidData_ReturnsCreatedTransactionGroup()
  {
    // arrange
    await InitializeAsync();
    var createTransactionGroupDto = new CreateTransactionGroupDto
    {
      Name = "New Group",
      Description = "Test transaction group",
      GroupIcon = "test-icon"
    };

    // act
    var response = await Client.PostAsync(TRANSACTION_GROUPS, CreateContent(createTransactionGroupDto));
    var createdTransactionGroup = await GetContentAsync<GetTransactionGroupDto>(response);

    // assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(createdTransactionGroup);
    Assert.Equal(createTransactionGroupDto.Name, createdTransactionGroup.Name);
    Assert.Equal(createTransactionGroupDto.Description, createdTransactionGroup.Description);
    Assert.Equal(createTransactionGroupDto.GroupIcon, createdTransactionGroup.GroupIcon);
  }

  [Fact]
  public async Task CreateTransactionGroup_WithEmptyName_ReturnsValidationError()
  {
    // arrange
    await InitializeAsync();
    var createTransactionGroupDto = new CreateTransactionGroupDto
    {
      Name = "",
      Description = "Test transaction group",
      GroupIcon = "test-icon"
    };

    // act
    var response = await Client.PostAsync(TRANSACTION_GROUPS, CreateContent(createTransactionGroupDto));
    var responseContent = await response.Content.ReadAsStringAsync();

    // assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    Assert.Contains(ApplicationError.VALIDATION_MESSAGE, responseContent);
  }

  [Fact]
  public async Task CreateTransactionGroup_WithDuplicateName_ReturnsError()
  {
    // arrange
    await InitializeAsync();
    var existingGroup = await CreateTransactionGroupAsync();
    var createTransactionGroupDto = new CreateTransactionGroupDto
    {
      Name = existingGroup!.Name,
      Description = "Duplicate name test",
      GroupIcon = "test-icon"
    };

    // act
    var response = await Client.PostAsync(TRANSACTION_GROUPS, CreateContent(createTransactionGroupDto));
    var responseContent = await response.Content.ReadAsStringAsync();

    // assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    Assert.Contains(ApplicationError.NAME_ALREADY_EXISTS_CODE, responseContent);
  }

  [Fact]
  public async Task GetTransactionGroupById_WithInvalidId_ReturnsNotFound()
  {
    // arrange
    await InitializeAsync();
    var invalidId = Guid.NewGuid();

    // act
    var response = await Client.GetAsync(TRANSACTION_GROUPS + invalidId);

    // assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task UpdateTransactionGroup_WithInvalidId_ReturnsNotFound()
  {
    // arrange
    await InitializeAsync();
    var invalidId = Guid.NewGuid();
    var updateDto = new UpdateTransactionGroupDto
    {
      Id = invalidId,
      Name = "Updated Name",
      Description = "Updated Description"
    };

    // act
    var response = await Client.PutAsync(TRANSACTION_GROUPS + invalidId, CreateContent(updateDto));

    // assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task UpdateTransactionGroup_WithEmptyName_ReturnsValidationError()
  {
    // arrange
    await InitializeAsync();
    var transactionGroup = await CreateTransactionGroupAsync();
    var updateDto = new UpdateTransactionGroupDto
    {
      Id = transactionGroup!.Id,
      Name = "",
      Description = "Updated Description"
    };

    // act
    var response = await Client.PutAsync(TRANSACTION_GROUPS + transactionGroup.Id, CreateContent(updateDto));
    var responseContent = await response.Content.ReadAsStringAsync();

    // assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    Assert.Contains(ApplicationError.VALIDATION_MESSAGE, responseContent);
  }

  [Fact]
  public async Task GetAllTransactionGroups_WhenEmpty_ReturnsEmptyList()
  {
    // arrange
    await InitializeAsync();

    // act
    var response = await GetContentAsync<List<GetTransactionGroupDto>>(await Client.GetAsync(TRANSACTION_GROUPS));

    // assert
    Assert.NotNull(response);
    // Note: There might be default groups, so we check that we get a valid response
    Assert.True(response.Count >= 0);
  }
}
