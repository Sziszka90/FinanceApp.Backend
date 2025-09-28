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
  public async Task Logout_ReturnsOk()
  {
    await InitializeAsync();
    var response = await Client.PostAsync(AUTH_ENDPOINT + "logout", null);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task Check_Authenticated_ReturnsTrue()
  {
    // arrange
    await InitializeAsync();
    var loginRequest = new LoginRequestDto
    {
      Email = "test_user90@example.com",
      Password = "TestPassword90."
    };

    // act
    var loginResult = await Client.PostAsync(AUTH_ENDPOINT + "login", CreateContent(loginRequest));
    Assert.Equal(HttpStatusCode.OK, loginResult.StatusCode);

    // Extract Token cookie from login response
    var tokenCookie = loginResult.Headers.TryGetValues("Set-Cookie", out var cookies)
      ? cookies.FirstOrDefault(c => c.StartsWith("Token="))
      : null;

    var checkRequest = new HttpRequestMessage(HttpMethod.Get, AUTH_ENDPOINT + "check");
    if (tokenCookie != null)
    {
      checkRequest.Headers.Add("Cookie", tokenCookie);
    }
    var checkResponse = await Client.SendAsync(checkRequest);
    var content = await checkResponse.Content.ReadAsStringAsync();

    // assert
    Assert.Equal(HttpStatusCode.OK, checkResponse.StatusCode);
    Assert.Contains("true", content.ToLower());
  }

  [Fact]
  public async Task Check_NotAuthenticated_ReturnsFalse()
  {
    // arrange
    await InitializeAsync();

    // act
    var checkResponse = await Client.GetAsync(AUTH_ENDPOINT + "check");
    var content = await checkResponse.Content.ReadAsStringAsync();

    // assert
    Assert.Equal(HttpStatusCode.OK, checkResponse.StatusCode);
    Assert.Contains("false", content.ToLower());
  }
}
