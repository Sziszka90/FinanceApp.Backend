using System.Net;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Enums;
using FinanceApp.Testing.Base;

namespace FinanceApp.Testing.User;

public class UserApi : TestBase
{
  [Fact]
  public async Task DeleteNotExistingUser_ReturnsNotFound()
  {
    // Arrange
    await InitializeAsync();
    var user = await CreateUserAsync();
    user!.Id = Guid.NewGuid();

    // Act
    var response = await Client.DeleteAsync(USERS + user!.Id);

    // Assert
    Assert.Equal(response.StatusCode, HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task DeleteUser_ReturnsNothing()
  {
    // Arrange
    await InitializeAsync();

    // Act
    var deleteResponse = await Client.DeleteAsync(USERS + CreatedUserId);
    var response = await GetContentAsync<GetUserDto>(await Client.GetAsync(USERS + CreatedUserId));

    // Assert
    Assert.Null(response);
    Assert.Equal(deleteResponse.StatusCode, HttpStatusCode.NoContent);
  }

  [Fact]
  public async Task GetUserById_ReturnsValidUser()
  {
    // Arrange
    await InitializeAsync();

    // Act
    var response = await GetContentAsync<GetUserDto>(await Client.GetAsync(USERS + CreatedUserId));

    // Assert
    Assert.Equal(CreatedUserId, response!.Id);
  }

  [Fact]
  public async Task UpdateUser_ReturnsUpdatedUser()
  {
    // Arrange
    await InitializeAsync();
    var updatedUser = new UpdateUserDto
    {
      Id = CreatedUserId,
      UserName = "updated_test_user_90",
      Password = "TestPassword95.",
      BaseCurrency = CurrencyEnum.USD
    };

    // Act
    await GetContentAsync<GetUserDto>(await Client.PutAsync(USERS, CreateContent(updatedUser)));
    var response = await GetContentAsync<GetUserDto>(await Client.GetAsync(USERS + CreatedUserId));

    // Assert
    Assert.Equal(CreatedUserId, response!.Id);
    Assert.Equal(updatedUser.UserName, response.UserName);
    Assert.Equal(updatedUser.BaseCurrency, response.BaseCurrency);
  }

  [Fact]
  public async Task UpdateUserInvalidPassword_ReturnsValidationError()
  {
    // Arrange
    await InitializeAsync();
    var updatedUser = new UpdateUserDto
    {
      Id = CreatedUserId,
      UserName = "updated_test_user_90",
      Password = "TestPassword",
      BaseCurrency = CurrencyEnum.EUR
    };

    // Act
    var response = await (await Client.PutAsync(USERS, CreateContent(updatedUser))).Content.ReadAsStringAsync();

    // Assert
    Assert.Contains(ApplicationError.VALIDATION_CODE, response);
  }
}