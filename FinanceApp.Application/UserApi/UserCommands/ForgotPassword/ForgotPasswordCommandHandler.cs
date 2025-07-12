using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.UserApi.UserCommands.ForgotPassword;

public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, Result>
{
  private readonly ISmtpEmailSender _smtpEmailSender;

  public ForgotPasswordCommandHandler(
    ISmtpEmailSender smtpEmailSender)
  {
    _smtpEmailSender = smtpEmailSender;
  }

  public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
  {
    await _smtpEmailSender.SendForgotPasswordAsync(request.EmailDto.Email);

    return Result.Success();
  }
}
