using System.Net;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.SavingDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Testing.Base;

namespace FinanceApp.Testing.Saving;

public class SavingApi : TestBase
{
  [Fact]
  public async Task DeleteNotExistingSaving_ReturnsNotFound()
  {
    // Arrange
    await InitializeAsync();
    var saving = await CreateSavingAsync();
    saving!.Id = Guid.NewGuid();

    // Act
    var response = await Client.DeleteAsync(SAVINGS + saving!.Id);

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task DeleteSaving_ReturnsNothing()
  {
    // Arrange
    await InitializeAsync();
    var saving = await CreateSavingAsync();

    // Act
    await Client.DeleteAsync(SAVINGS + saving!.Id);
    var response = await GetContentAsync<GetSavingDto>(await Client.GetAsync(SAVINGS + saving!.Id));

    // Assert
    Assert.Null(response);
  }

  [Fact]
  public async Task GetAllSaving_ReturnsValidSaving()
  {
    // Arrange
    await InitializeAsync();
    var saving = await CreateSavingAsync();

    // Act
    var response = await GetContentAsync<List<GetSavingDto>>(await Client.GetAsync(SAVINGS));

    // Assert
    Assert.Equal(saving!.Id, response![0].Id);
  }

  [Fact]
  public async Task GetSavingById_ReturnsValidSaving()
  {
    // Arrange
    await InitializeAsync();
    var saving = await CreateSavingAsync();

    // Act
    var response = await GetContentAsync<GetSavingDto>(await Client.GetAsync(SAVINGS + saving!.Id));

    // Assert
    Assert.Equal(saving!.Id, response!.Id);
  }

  [Fact]
  public async Task UpdateSaving_ReturnsUpdatedSaving()
  {
    // Arrange
    await InitializeAsync();
    var saving = await CreateSavingAsync();
    var updatedSaving = new UpdateSavingDto
    {
      Id = saving!.Id,
      Name = "Updated Name",
      Amount = saving.Amount,
      Description = "Updated Description"
    };

    // Act
    await GetContentAsync<GetSavingDto>(await Client.PutAsync(SAVINGS, CreateContent(updatedSaving)));
    var response = await GetContentAsync<GetSavingDto>(await Client.GetAsync(SAVINGS + saving!.Id));

    // Assert
    Assert.Equal(saving!.Id, response!.Id);
    Assert.Equal(updatedSaving.Name, response.Name);
    Assert.Equal(updatedSaving.Amount.Amount, response.Amount.Amount);
  }

  [Fact]
  public async Task UpdateSavingNegativeValue_ReturnsValidationError()
  {
    // Arrange
    await InitializeAsync();
    var saving = await CreateSavingAsync();
    var updatedSaving = new UpdateSavingDto
    {
      Id = saving!.Id,
      Name = "Updated Name",
      Amount = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = -100
      },
      Description = "Updated Description"
    };


    // Act
    var response = await Client.PutAsync(SAVINGS, CreateContent(updatedSaving));
    var responseContentAsString = await response.Content.ReadAsStringAsync();

    // Assert
    Assert.Contains(ApplicationError.VALIDATION_MESSAGE, responseContentAsString);
  }
}
