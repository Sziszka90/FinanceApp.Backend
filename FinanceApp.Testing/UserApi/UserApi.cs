using System.Net;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Domain.Enums;
using FinanceApp.Testing.Base;

namespace FinanceApp.Testing.UserApi;

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
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
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
      BaseCurrency = CurrencyEnum.USD
    };

    // Act
    await GetContentAsync<GetUserDto>(await Client.PutAsync(USERS, CreateContent(updatedUser)));
    var response = await GetContentAsync<GetUserDto>(await Client.GetAsync(USERS + CreatedUserId));

    // Assert
    Assert.Equal(CreatedUserId, response!.Id);
    Assert.Equal(updatedUser.BaseCurrency, response.BaseCurrency);
  }
}
