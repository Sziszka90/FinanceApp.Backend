using System.Net;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.IncomeTransactionDtos;
using FinanceApp.Application.Dtos.IncomeTransactionGroupDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Testing.Base;

namespace FinanceApp.Testing.IncomeTransaction;

public class IncomeTransactionApi : TestBase
{
  [Fact]
  public async Task CreateIncome_WhenTransactionGroupDoesNotExists_ReturnsNotExists()
  {
    // Arrange
    await InitializeAsync();
    var incomeContent = CreateContent(new CreateIncomeTransactionDto
    {
      Name = "TestIncome",
      Description = "Test Income",
      Value = new Money
      {
        Amount = 100,
        Currency = CurrencyEnum.HUF
      },
      DueDate = new DateTimeOffset(),
      TransactionGroupId = new Guid()
    });

    // Act
    var response = await Client.PostAsync(INCOME_TRANSACTIONS, incomeContent);
    var content = await response.Content.ReadAsStringAsync();

    // Assert
    Assert.Contains(ApplicationError.TRANSACTION_GROUP_NOT_EXISTS_CODE, content);
  }

  [Fact]
  public async Task DeleteIncome_DoesNotDeleteIncomeGroup()
  {
    // Arrange
    await InitializeAsync();
    var income = await CreateIncomeAsync();
    await Client.DeleteAsync(INCOME_TRANSACTIONS + income!.Id);

    // Act
    var response = await GetContentAsync<GetIncomeTransactionGroupDto>(await Client.GetAsync(INCOME_TRANSACTION_GROUPS + income!.TransactionGroup!.Id));

    // Assert
    Assert.Equal(response!.Id, income.TransactionGroup!.Id);
  }

  [Fact]
  public async Task DeleteIncome_ReturnsNothing()
  {
    // Arrange
    await InitializeAsync();
    var income = await CreateIncomeAsync();

    // Act
    await Client.DeleteAsync(INCOME_TRANSACTIONS + income!.Id);
    var response = await GetContentAsync<GetIncomeTransactionDto>(await Client.GetAsync(INCOME_TRANSACTIONS + income!.Id));

    // Assert
    Assert.Null(response);
  }

  [Fact]
  public async Task DeleteIncomeAndIncomeGroup_ReturnsNoContent()
  {
    // Arrange
    await InitializeAsync();
    var income = await CreateIncomeAsync();


    // Act

    var responseIncome = await Client.DeleteAsync(INCOME_TRANSACTIONS + income!.Id);
    var responseIncomeGroup = await Client.DeleteAsync(INCOME_TRANSACTION_GROUPS + income!.TransactionGroup!.Id);

    // Assert
    Assert.Equal(responseIncome.StatusCode, HttpStatusCode.NoContent);
    Assert.Equal(responseIncomeGroup.StatusCode, HttpStatusCode.NoContent);
  }

  [Fact]
  public async Task DeleteNotExistingIncome_ReturnsNotFound()
  {
    // Arrange
    await InitializeAsync();
    var income = await CreateIncomeAsync();
    income!.Id = Guid.NewGuid();

    // Act
    var response = await Client.DeleteAsync(INCOME_TRANSACTIONS + income!.Id);

    // Assert
    Assert.Equal(response.StatusCode, HttpStatusCode.NotFound);
  }


  [Fact]
  public async Task GetAllIncome_ReturnsValidIncome()
  {
    // Arrange
    await InitializeAsync();
    var income = await CreateIncomeAsync();

    // Act
    var response = await GetContentAsync<List<GetIncomeTransactionDto>>(await Client.GetAsync(INCOME_TRANSACTIONS));

    // Assert
    Assert.Equal(income!.Id, response![0].Id);
  }

  [Fact]
  public async Task GetAllIncomeSummary_ReturnsAllIncomeSummary()
  {
    // Arrange
    await InitializeAsync();
    var incomes = await CreateMultipleIncomeAsync();

    // Act
    var response = await Client.GetAsync(INCOME_TRANSACTIONS_SUMMARY);
    var sum = await GetContentAsync<Money>(response);

    // Assert
    Assert.True(sum!.Amount > incomes[0].Value.Amount);
  }

  [Fact]
  public async Task GetIncomeById_ReturnsValidIncome()
  {
    // Arrange
    await InitializeAsync();
    var income = await CreateIncomeAsync();

    // Act
    var response = await GetContentAsync<GetIncomeTransactionDto>(await Client.GetAsync(INCOME_TRANSACTIONS + income!.Id));

    // Assert
    Assert.Equal(income!.Id, response!.Id);
  }

  [Fact]
  public async Task UpdateIncome_ReturnsUpdatedIncome()
  {
    // Arrange
    await InitializeAsync();
    var income = await CreateIncomeAsync();
    var updatedIncome = new UpdateIncomeTransactionDto
    {
      Id = income!.Id,
      Name = "Updated Name",
      Description = "Updated Description",
      Value = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = 200
      },
      DueDate = income.DueDate,
      TransactionGroupId = income.TransactionGroup!.Id
    };

    // Act
    await GetContentAsync<GetIncomeTransactionDto>(await Client.PutAsync(INCOME_TRANSACTIONS, CreateContent(updatedIncome)));
    var response = await GetContentAsync<GetIncomeTransactionDto>(await Client.GetAsync(INCOME_TRANSACTIONS + income!.Id));

    // Assert
    Assert.Equal(income!.Id, response!.Id);
    Assert.Equal(updatedIncome.Name, response.Name);
    Assert.Equal(updatedIncome.Value.Amount, response.Value.Amount);
  }

  [Fact]
  public async Task UpdateIncomeNegativeValue_ReturnsValidationError()
  {
    // Arrange
    await InitializeAsync();
    var income = await CreateIncomeAsync();
    var updatedIncome = new UpdateIncomeTransactionDto
    {
      Id = income!.Id,
      Name = "Updated Name",
      Description = "Updated Description",
      Value = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = -200
      },
      DueDate = income.DueDate,
      TransactionGroupId = income.TransactionGroup!.Id
    };

    // Act
    var response = await Client.PutAsync(INCOME_TRANSACTIONS, CreateContent(updatedIncome));
    var responseContentAsString = await response.Content.ReadAsStringAsync();

    // Assert
    Assert.Contains(ApplicationError.VALIDATION_MESSAGE, responseContentAsString);
  }
}