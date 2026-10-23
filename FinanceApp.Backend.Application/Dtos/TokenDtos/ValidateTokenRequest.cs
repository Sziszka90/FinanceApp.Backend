using FinanceApp.Backend.Domain.Enums;

namespace FinanceApp.Backend.Application.Dtos.TokenDtos;

public class ValidateTokenRequest
{
  public string Token { get; set; } = string.Empty;
  public TokenType TokenType { get; set; } = TokenType.Login;
}
