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
}
