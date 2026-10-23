using System.IdentityModel.Tokens.Jwt;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Options;
using Microsoft.Extensions.Options;

namespace FinanceApp.Backend.Testing.Unit.ServiceTests.Application;

public class JwtServiceTests
{
  private JwtService CreateService(string secret = "0123456789abcdef0123456789abcdef", string issuer = "issuer", string audience = "audience")
  {
    var settings = new AuthenticationSettings { SecretKey = secret, Issuer = issuer, Audience = audience };
    var options = Options.Create(settings);
    return new JwtService(options);
  }

  [Fact]
  public void GenerateToken_ReturnsValidJwt()
  {
    // arrange
    var service = CreateService();

    // act
    var token = service.GenerateToken("test@example.com");

    // assert
    Assert.False(string.IsNullOrWhiteSpace(token));
    var handler = new JwtSecurityTokenHandler();
    var jwt = handler.ReadJwtToken(token);
    Assert.Equal("issuer", jwt.Issuer);
    Assert.Equal("audience", jwt.Audiences.First());
  }

  [Fact]
  public void ValidateToken_ReturnsTrueForValidToken()
  {
    // arrange
    var service = CreateService();
    var token = service.GenerateToken("test@example.com");

    // act
    var result = service.ValidateToken(token);

    // assert
    Assert.True(result);
  }

  [Fact]
  public void ValidateToken_ReturnsFalseForInvalidToken()
  {
    // arrange
    var service = CreateService();

    // act
    var result = service.ValidateToken("invalid.token.value");

    // assert
    Assert.False(result);
  }

  [Fact]
  public void GenerateEmailConfirmationToken_ReturnsValidJwt()
  {
    // arrange
    var service = CreateService();

    // act
    var token = service.GenerateEmailConfirmationToken("test@example.com");

    // assert
    Assert.False(string.IsNullOrWhiteSpace(token));
    var handler = new JwtSecurityTokenHandler();
    var jwt = handler.ReadJwtToken(token);
    Assert.Equal("issuer", jwt.Issuer);
    Assert.Equal("audience", jwt.Audiences.First());
    Assert.Equal("test@example.com", jwt.Subject);
  }

  [Fact]
  public void GenerateEmailConfirmationToken_HasLongerExpirationThanRegularToken()
  {
    // arrange
    var service = CreateService();

    // act
    var confirmationToken = service.GenerateEmailConfirmationToken("test@example.com");
    var regularToken = service.GenerateToken("test@example.com");

    // assert
    var handler = new JwtSecurityTokenHandler();
    var confirmationJwt = handler.ReadJwtToken(confirmationToken);
    var regularJwt = handler.ReadJwtToken(regularToken);

    // Email confirmation token expires in 60 minutes, regular token in 5 minutes
    Assert.True(confirmationJwt.ValidTo > regularJwt.ValidTo);
  }

  [Fact]
  public void GenerateEmailConfirmationToken_IsValidatedSuccessfully()
  {
    // arrange
    var service = CreateService();
    var token = service.GenerateEmailConfirmationToken("test@example.com");

    // act
    var result = service.ValidateToken(token);

    // assert
    Assert.True(result);
  }

  [Fact]
  public void GenerateEmailConfirmationToken_ContainsEmailInSubject()
  {
    // arrange
    var service = CreateService();
    var email = "user@example.com";

    // act
    var token = service.GenerateEmailConfirmationToken(email);

    // assert
    var handler = new JwtSecurityTokenHandler();
    var jwt = handler.ReadJwtToken(token);
    var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
    Assert.NotNull(subClaim);
    Assert.Equal(email, subClaim.Value);
  }
}
