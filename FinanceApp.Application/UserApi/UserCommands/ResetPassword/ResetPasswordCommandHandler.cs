using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.UserApi.UserCommands.ResetPassword;

public class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand, Result>
{
  private readonly IJwtService _jwtService;

  public ResetPasswordCommandHandler(
    IJwtService jwtService)
  {
    _jwtService = jwtService;
  }

  public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
  {

    if (request.token is null)
    {
      return Result.Failure(ApplicationError.InvalidTokenError());
    }

    var validationResult = _jwtService.ValidateToken(request.token);

    if (!validationResult)
    {
      return Result.Failure(ApplicationError.InvalidTokenError());
    }

    _jwtService.InvalidateToken(request.token);

    return Result.Success();
  }
}
