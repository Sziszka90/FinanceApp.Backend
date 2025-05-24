using System.Net;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.InvestmentDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Testing.Base;

namespace FinanceApp.Testing.Investment;

public class InvestmentApi : TestBase
{
  [Fact]
  public async Task DeleteInvestment_ReturnsNothing()
  {
    // Arrange
    await InitializeAsync();
    var investment = await CreateInvestmentAsync();

    // Act
    await Client.DeleteAsync(INVESTMENTS + investment!.Id);
    var response = await GetContentAsync<GetInvestmentDto>(await Client.GetAsync(INVESTMENTS + investment!.Id));

    // Assert
    Assert.Null(response);
  }

  [Fact]
  public async Task DeleteNotExistingInvestment_ReturnsNotFound()
  {
    // Arrange
    await InitializeAsync();
    var investment = await CreateInvestmentAsync();
    investment!.Id = Guid.NewGuid();

    // Act
    var response = await Client.DeleteAsync(INVESTMENTS + investment!.Id);

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task GetAllInvestment_ReturnsValidInvestment()
  {
    // Arrange
    await InitializeAsync();
    var investment = await CreateInvestmentAsync();

    // Act
    var response = await GetContentAsync<List<GetInvestmentDto>>(await Client.GetAsync(INVESTMENTS));

    // Assert
    Assert.Equal(investment!.Id, response![0].Id);
  }

  [Fact]
  public async Task GetInvestmentById_ReturnsValidInvestment()
  {
    // Arrange
    await InitializeAsync();
    var investment = await CreateInvestmentAsync();

    // Act
    var response = await GetContentAsync<GetInvestmentDto>(await Client.GetAsync(INVESTMENTS + investment!.Id));

    // Assert
    Assert.Equal(investment!.Id, response!.Id);
  }

  [Fact]
  public async Task UpdateInvestment_ReturnsUpdatedInvestment()
  {
    // Arrange
    await InitializeAsync();
    var investment = await CreateInvestmentAsync();
    var updatedInvestment = new UpdateInvestmentDto
    {
      Id = investment!.Id,
      Name = "Updated Name",
      Amount = investment.Amount,
      Description = "Updated Description"
    };

    // Act
    await GetContentAsync<GetInvestmentDto>(await Client.PutAsync(INVESTMENTS, CreateContent(updatedInvestment)));
    var response = await GetContentAsync<GetInvestmentDto>(await Client.GetAsync(INVESTMENTS + investment!.Id));

    // Assert
    Assert.Equal(investment!.Id, response!.Id);
    Assert.Equal(updatedInvestment.Name, response.Name);
    Assert.Equal(updatedInvestment.Amount.Amount, response.Amount.Amount);
  }

  [Fact]
  public async Task UpdateInvestmentNegativeValue_ReturnsValidationError()
  {
    // Arrange
    await InitializeAsync();
    var investment = await CreateInvestmentAsync();
    var updatedInvestment = new UpdateInvestmentDto
    {
      Id = investment!.Id,
      Name = "Updated Name",
      Amount = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = -100
      },
      Description = "Updated Description"
    };


    // Act
    var response = await Client.PutAsync(INVESTMENTS, CreateContent(updatedInvestment));
    var responseContentAsString = await response.Content.ReadAsStringAsync();

    // Assert
    Assert.Contains(ApplicationError.VALIDATION_MESSAGE, responseContentAsString);
  }
}
