using System.Net;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.IncomeTransactionDtos;
using FinanceApp.Application.Dtos.IncomeTransactionGroupDtos;
using FinanceApp.Testing.Base;

namespace FinanceApp.Testing.IncomeTransactionGroup;

public class IncomeTransactionGroupApi : TestBase
{
  [Fact]
  public async Task DeleteIncomeGroup_ReturnsNothing()
  {
    // Arrange
    await InitializeAsync();
    var incomeGroup = await CreateIncomeTransactionGroupAsync();

    // Act
    await Client.DeleteAsync(INCOME_TRANSACTION_GROUPS + incomeGroup!.Id);
    var response = await GetContentAsync<GetIncomeTransactionGroupDto>(await Client.GetAsync(INCOME_TRANSACTION_GROUPS + incomeGroup!.Id));

    // Assert
    Assert.Null(response);
  }

  [Fact]
  public async Task DeleteNotExistingIncomeGroup_ReturnsNotFound()
  {
    // Arrange
    await InitializeAsync();
    var incomeGroup = await CreateIncomeTransactionGroupAsync();
    incomeGroup!.Id = Guid.NewGuid();

    // Act
    var response = await Client.DeleteAsync(INCOME_TRANSACTION_GROUPS + incomeGroup!.Id);

    // Assert
    Assert.Equal(response.StatusCode, HttpStatusCode.NotFound);
  }


  [Fact]
  public async Task GetAllIncomeGroup_ReturnsValidIncomeGroup()
  {
    // Arrange
    await InitializeAsync();
    var incomeGroup = await CreateIncomeTransactionGroupAsync();

    // Act
    var response = await GetContentAsync<List<GetIncomeTransactionGroupDto>>(await Client.GetAsync(INCOME_TRANSACTION_GROUPS));

    // Assert
    Assert.Equal(incomeGroup!.Id, response![0].Id);
  }

  [Fact]
  public async Task GetIncomeGroupById_ReturnsValidIncomeGroup()
  {
    // Arrange
    await InitializeAsync();
    var incomeGroup = await CreateIncomeTransactionGroupAsync();

    // Act
    var response = await GetContentAsync<GetIncomeTransactionGroupDto>(await Client.GetAsync(INCOME_TRANSACTION_GROUPS + incomeGroup!.Id));

    // Assert
    Assert.Equal(incomeGroup!.Id, response!.Id);
  }

  [Fact]
  public async Task UpdateIncomeGroup_ReturnsUpdatedIncomeGroup()
  {
    // Arrange
    await InitializeAsync();
    var incomeGroup = await CreateIncomeTransactionGroupAsync();
    var updatedIncomeGroup = new UpdateIncomeTransactionGroupDto
    {
      Id = incomeGroup!.Id,
      Name = "Updated Name",
      Description = "Updated Description",
      Icon = incomeGroup.Icon
    };

    // Act
    await GetContentAsync<GetIncomeTransactionGroupDto>(await Client.PutAsync(INCOME_TRANSACTION_GROUPS, CreateContent(updatedIncomeGroup)));
    var response = await GetContentAsync<GetIncomeTransactionDto>(await Client.GetAsync(INCOME_TRANSACTION_GROUPS + incomeGroup!.Id));

    // Assert
    Assert.Equal(incomeGroup!.Id, response!.Id);
    Assert.Equal(updatedIncomeGroup.Name, response.Name);
  }
}