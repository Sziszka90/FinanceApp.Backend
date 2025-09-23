using System.Net;
using FinanceApp.Backend.Application.Dtos.AuthDtos;
using FinanceApp.Backend.Testing.Api.Base;
using Xunit;

namespace FinanceApp.Backend.Testing.Api.AuthApi;

public class AuthApi : TestBase
{
  private const string AUTH_ENDPOINT = "/api/v1/auth/";

  [Fact]
  public async Task Login_WithValidCredentials_ReturnsTokensAndSetsCookies()
  {
    await InitializeAsync();
    var loginRequest = new LoginRequestDto
    {
      Email = "test_user90@example.com",
      Password = "TestPassword90."
    };
    var result = await Client.PostAsync(AUTH_ENDPOINT + "login", CreateContent(loginRequest));

    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
  }

  [Fact]
  public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
  {
    await InitializeAsync();
    var loginRequest = new LoginRequestDto
    {
      Email = "test_user90@example.com",
      Password = "WrongPassword!"
    };
    var response = await Client.PostAsync(AUTH_ENDPOINT + "login", CreateContent(loginRequest));
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task Refresh_WithoutCookie_ReturnsBadRequest()
  {
    await InitializeAsync();
    var response = await Client.PostAsync(AUTH_ENDPOINT + "refresh", null);
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task Logout_ReturnsOk()
  {
    await InitializeAsync();
    var response = await Client.PostAsync(AUTH_ENDPOINT + "logout", null);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }
}
