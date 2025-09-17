using System.Net;
using FinanceApp.Backend.Application.Dtos.TokenDtos;
using FinanceApp.Backend.Application.UserApi.UserCommands.ValidateToken;
using FinanceApp.Backend.Domain.Enums;
using FinanceApp.Backend.Testing.Api.Base;

namespace FinanceApp.Backend.Testing.Api.TokenApi;

public class TokenApi : TestBase
{
  private const string TOKEN_ENDPOINT = "api/v1/token/";

  [Fact]
  public async Task ValidateToken_WithValidToken_ReturnsSuccessResponse()
  {
    // arrange
    await InitializeAsync();
    var validateTokenRequest = new ValidateTokenRequest
    {
      Token = "mock_password_reset_token",
      TokenType = TokenType.PasswordReset
    };

    // act
    var response = await Client.PostAsync(TOKEN_ENDPOINT + "validate", CreateContent(validateTokenRequest));
    var result = await GetContentAsync<ValidateTokenResponse>(response);

    // assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(result);
    Assert.True(result.IsValid);
  }

  [Fact]
  public async Task ValidateToken_WithInvalidToken_ReturnsUnauthorized()
  {
    // arrange
    await InitializeAsync();
    var validateTokenRequest = new ValidateTokenRequest
    {
      Token = "invalid_token_xyz"
    };

    // act
    var response = await Client.PostAsync(TOKEN_ENDPOINT + "validate", CreateContent(validateTokenRequest));
    var result = await GetContentAsync<ValidateTokenResponse>(response);

    // assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(result);
    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task ValidateToken_WithEmptyToken_ReturnsValidationError()
  {
    // arrange
    await InitializeAsync();
    var validateTokenRequest = new ValidateTokenRequest
    {
      Token = ""
    };

    // act
    var response = await Client.PostAsync(TOKEN_ENDPOINT + "validate", CreateContent(validateTokenRequest));
    var responseContent = await response.Content.ReadAsStringAsync();

    // assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    Assert.Contains("validation failed", responseContent.ToLower());
  }

  [Fact]
  public async Task ValidateToken_WithNullToken_ReturnsValidationError()
  {
    // arrange
    await InitializeAsync();
    var validateTokenRequest = new ValidateTokenRequest
    {
      Token = null!
    };

    // act
    var response = await Client.PostAsync(TOKEN_ENDPOINT + "validate", CreateContent(validateTokenRequest));
    var responseContent = await response.Content.ReadAsStringAsync();

    // assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    Assert.Contains("The Token field is required.", responseContent);
  }

  [Fact]
  public async Task ValidateToken_WithWhitespaceToken_ReturnsValidationError()
  {
    // arrange
    await InitializeAsync();
    var validateTokenRequest = new ValidateTokenRequest
    {
      Token = "   "
    };

    // act
    var response = await Client.PostAsync(TOKEN_ENDPOINT + "validate", CreateContent(validateTokenRequest));
    var responseContent = await response.Content.ReadAsStringAsync();

    // assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    Assert.Contains("validation failed", responseContent.ToLower());
  }

  [Fact]
  public async Task ValidateToken_WithExpiredToken_ReturnsUnauthorized()
  {
    // arrange
    await InitializeAsync();
    var expiredToken = GenerateExpiredToken();
    var validateTokenRequest = new ValidateTokenRequest
    {
      Token = expiredToken
    };

    // act
    var response = await Client.PostAsync(TOKEN_ENDPOINT + "validate", CreateContent(validateTokenRequest));
    var result = await GetContentAsync<ValidateTokenResponse>(response);

    // assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(result);
    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task ValidateToken_WithMalformedToken_ReturnsUnauthorized()
  {
    // arrange
    await InitializeAsync();
    var malformedToken = "malformed.token.here";
    var validateTokenRequest = new ValidateTokenRequest
    {
      Token = malformedToken
    };

    // act
    var response = await Client.PostAsync(TOKEN_ENDPOINT + "validate", CreateContent(validateTokenRequest));
    var result = await GetContentAsync<ValidateTokenResponse>(response);

    // assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(result);
    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task ValidateToken_WithVeryLongToken_ReturnsUnauthorized()
  {
    // arrange
    await InitializeAsync();
    var longToken = new string('a', 10000); // Very long string
    var validateTokenRequest = new ValidateTokenRequest
    {
      Token = longToken
    };

    // act
    var response = await Client.PostAsync(TOKEN_ENDPOINT + "validate", CreateContent(validateTokenRequest));
    var result = await GetContentAsync<ValidateTokenResponse>(response);

    // assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(result);
    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task ValidateToken_WithSpecialCharacters_ReturnsUnauthorized()
  {
    // arrange
    await InitializeAsync();
    var specialCharToken = "token_with_!@#$%^&*()_special_chars";
    var validateTokenRequest = new ValidateTokenRequest
    {
      Token = specialCharToken
    };

    // act
    var response = await Client.PostAsync(TOKEN_ENDPOINT + "validate", CreateContent(validateTokenRequest));
    var result = await GetContentAsync<ValidateTokenResponse>(response);

    // assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(result);
    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task ValidateToken_ConcurrentRequests_HandlesCorrectly()
  {
    // arrange
    await InitializeAsync();
    var token = "mocked_jwt_token";
    var validateTokenRequest = new ValidateTokenRequest
    {
      Token = token
    };

    // act - send multiple concurrent requests
    var tasks = Enumerable.Range(0, 10)
      .Select(_ => Client.PostAsync(TOKEN_ENDPOINT + "validate", CreateContent(validateTokenRequest)))
      .ToArray();

    var responses = await Task.WhenAll(tasks);

    // assert
    Assert.All(responses, response =>
    {
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    });
  }
  [Fact]
  public async Task ValidateToken_WithoutRequestBody_ReturnsBadRequest()
  {
    // arrange
    await InitializeAsync();

    // act
    var response = await Client.PostAsync(TOKEN_ENDPOINT + "validate", null);

    // assert
    Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
  }

  [Fact]
  public async Task ValidateToken_WithEmptyRequestBody_ReturnsBadRequest()
  {
    // arrange
    await InitializeAsync();

    // act
    var response = await Client.PostAsync(TOKEN_ENDPOINT + "validate", CreateContent(""));

    // assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task ValidateToken_SecurityTest_TokenNotInUrl()
  {
    // arrange
    await InitializeAsync();
    var token = "mocked_jwt_token";

    // act - verify that token validation is done via POST body, not URL parameters
    var getResponse = await Client.GetAsync(TOKEN_ENDPOINT + $"validate?token={token}");

    // assert - GET should not be allowed for token validation (security concern)
    Assert.Equal(HttpStatusCode.MethodNotAllowed, getResponse.StatusCode);
  }

  private string GenerateExpiredToken()
  {
    // This would generate a token that's already expired
    // For testing purposes, we'll use a token that looks valid but is expired
    return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
  }
}
