using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace FinanceApp.Application.Services;

public class JwtService : IJwtService
{
  private readonly AuthenticationSettings _authenticationSettings;
  private static readonly HashSet<string> _invalidatedTokens = new();


  public JwtService(IOptions<AuthenticationSettings> authenticationOptions)
  {
    _authenticationSettings = authenticationOptions.Value;
  }

  public string GenerateToken(string username)
  {
    var claims = new[]
    {
      new Claim(JwtRegisteredClaimNames.Sub, username),
      new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid()
                                                 .ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.SecretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
      _authenticationSettings.Issuer,
      _authenticationSettings.Audience,
      claims,
      expires: DateTime.UtcNow.AddHours(1),
      signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  public bool ValidateToken(string token)
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(_authenticationSettings.SecretKey);

    try
    {
      var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
      {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = _authenticationSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = _authenticationSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
      }, out SecurityToken validatedToken);

      return true;
    }
    catch
    {
      return false;
    }
  }

  public void InvalidateToken(string token)
  {
    _invalidatedTokens.Add(token);
  }

  public bool IsTokenInvalidated(string token)
  {
    return _invalidatedTokens.Contains(token);
  }

  public string? GetUserNameFromToken(string token)
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(_authenticationSettings.SecretKey);
    try
    {
      var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
      {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = _authenticationSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = _authenticationSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
      }, out SecurityToken validatedToken);

      // Get the username from the 'sub' claim
      return principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    }
    catch
    {
      return null;
    }
  }
}
