using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.User.UserCommands;

public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, Result>
{
  private readonly IUserRepository _userRepository;
  private readonly ISmtpEmailSender _smtpEmailSender;

  public ForgotPasswordCommandHandler(
    IUserRepository userRepository,
    ISmtpEmailSender smtpEmailSender)
  {
    _userRepository = userRepository;
    _smtpEmailSender = smtpEmailSender;
  }

  public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
  {

    await _smtpEmailSender.SendResetPasswordAsync(request.Email);

    return Result.Success();
  }
}
