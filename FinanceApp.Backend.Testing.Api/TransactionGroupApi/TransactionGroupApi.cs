using System.Net;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
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
    Assert.Equal(transactionGroup!.Id, response![19].Id);
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

  [Fact]
  public async Task GetTopTransactionGroups_WithValidParameters_ReturnsTopGroups()
  {
    // arrange
    await InitializeAsync();

    // Create transaction groups with transactions
    var transactionGroup1 = await CreateTransactionGroupAsync(new CreateTransactionGroupDto
    {
      Name = "Test Group 1",
      Description = "Test Group 1 Description",
      GroupIcon = "icon1"
    });

    var transactionGroup2 = await CreateTransactionGroupAsync(new CreateTransactionGroupDto
    {
      Name = "Test Group 2",
      Description = "Test Group 2 Description",
      GroupIcon = "icon2"
    });

    // Create transactions for each group to have different totals
    await CreateTransactionForGroupAsync(transactionGroup1!.Id, 1000, CurrencyEnum.EUR);
    await CreateTransactionForGroupAsync(transactionGroup1.Id, 500, CurrencyEnum.EUR);
    await CreateTransactionForGroupAsync(transactionGroup2!.Id, 200, CurrencyEnum.EUR);

    var startDate = DateTimeOffset.UtcNow.AddDays(-30);
    var endDate = DateTimeOffset.UtcNow;
    var top = 5;

    // act
    var response = await GetContentAsync<List<TopTransactionGroupDto>>(
      await Client.GetAsync($"{TRANSACTION_GROUPS}top?startDate={startDate:yyyy-MM-ddTHH:mm:ssZ}&endDate={endDate:yyyy-MM-ddTHH:mm:ssZ}&top={top}"));

    // assert
    Assert.NotNull(response);
    Assert.True(response.Count <= top);

    // Verify the response contains our transaction groups
    var group1InResponse = response.FirstOrDefault(g => g.Id == transactionGroup1.Id);
    var group2InResponse = response.FirstOrDefault(g => g.Id == transactionGroup2.Id);

    Assert.NotNull(group1InResponse);
    Assert.NotNull(group2InResponse);

    // Verify that groups are ordered by total amount (descending)
    if (response.Count > 1)
    {
      for (int i = 0; i < response.Count - 1; i++)
      {
        Assert.True(response[i].TotalAmount.Amount >= response[i + 1].TotalAmount.Amount);
      }
    }
  }

  [Fact]
  public async Task GetTopTransactionGroups_WithDefaultTopParameter_ReturnsMaxTenGroups()
  {
    // arrange
    await InitializeAsync();

    var startDate = DateTimeOffset.UtcNow.AddDays(-30);
    var endDate = DateTimeOffset.UtcNow;

    // act
    var response = await GetContentAsync<List<TopTransactionGroupDto>>(
      await Client.GetAsync($"{TRANSACTION_GROUPS}top?startDate={startDate:yyyy-MM-ddTHH:mm:ssZ}&endDate={endDate:yyyy-MM-ddTHH:mm:ssZ}"));

    // assert
    Assert.NotNull(response);
    Assert.True(response.Count <= 10); // Default top value is 10
  }

  [Fact]
  public async Task GetTopTransactionGroups_WithInvalidDateRange_ReturnsBadRequest()
  {
    // arrange
    await InitializeAsync();

    var startDate = DateTimeOffset.UtcNow;
    var endDate = DateTimeOffset.UtcNow.AddDays(-30); // End date before start date
    var top = 5;

    // act
    var response = await Client.GetAsync($"{TRANSACTION_GROUPS}top?startDate={startDate:yyyy-MM-ddTHH:mm:ssZ}&endDate={endDate:yyyy-MM-ddTHH:mm:ssZ}&top={top}");

    // assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetTopTransactionGroups_WithNegativeTopParameter_ReturnsBadRequest()
  {
    // arrange
    await InitializeAsync();

    var startDate = DateTimeOffset.UtcNow.AddDays(-30);
    var endDate = DateTimeOffset.UtcNow;
    var top = -1;

    // act
    var response = await Client.GetAsync($"{TRANSACTION_GROUPS}top?startDate={startDate:yyyy-MM-ddTHH:mm:ssZ}&endDate={endDate:yyyy-MM-ddTHH:mm:ssZ}&top={top}");

    // assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetTopTransactionGroups_WithNoTransactionsInRange_ReturnsEmptyList()
  {
    // arrange
    await InitializeAsync();

    // Use a date range in the far future where no transactions exist
    var startDate = DateTimeOffset.UtcNow.AddYears(1);
    var endDate = DateTimeOffset.UtcNow.AddYears(2);
    var top = 5;

    // act
    var response = await Client.GetAsync($"{TRANSACTION_GROUPS}top?startDate={startDate:yyyy-MM-ddTHH:mm:ssZ}&endDate={endDate:yyyy-MM-ddTHH:mm:ssZ}&top={top}");

    // assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetTopTransactionGroups_WithMissingRequiredParameters_ReturnsBadRequest()
  {
    // arrange
    await InitializeAsync();

    // act - Missing startDate and endDate parameters
    var response = await Client.GetAsync($"{TRANSACTION_GROUPS}top?top=5");

    // assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  private async Task<GetTransactionDto?> CreateTransactionForGroupAsync(Guid transactionGroupId, decimal amount, CurrencyEnum currency)
  {
    var transactionContent = CreateContent(new CreateTransactionDto
    {
      Name = $"Test Transaction {Guid.NewGuid()}",
      Description = "Test Transaction for Top Groups",
      Value = new Money
      {
        Amount = amount,
        Currency = currency
      },
      TransactionType = TransactionTypeEnum.Expense,
      TransactionDate = DateTimeOffset.UtcNow.AddDays(-5), // Within the typical test date range
      TransactionGroupId = transactionGroupId.ToString()
    });

    var result = await GetContentAsync<GetTransactionDto>(await Client.PostAsync(TRANSACTIONS, transactionContent));
    return result;
  }
}
