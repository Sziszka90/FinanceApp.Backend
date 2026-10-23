namespace FinanceApp.Backend.Application.Dtos.UserDtos;

public class ResendEmailConfirmationResponse
{
  public bool IsSuccess { get; set; }
  public string Message { get; set; } = string.Empty;
}
