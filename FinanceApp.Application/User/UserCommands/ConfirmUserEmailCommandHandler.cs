using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.User.UserCommands;

public class ConfirmUserEmailCommandHandler : ICommandHandler<ConfirmUserEmailCommand, Result>
{
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IJwtService _jwtService;

  public ConfirmUserEmailCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IJwtService jwtService)
  {
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
    _jwtService = jwtService;
  }

  public async Task<Result> Handle(ConfirmUserEmailCommand request, CancellationToken cancellationToken)
  {
    if (request.token is null)
    {
      return Result.Failure(ApplicationError.InvalidTokenError());
    }

    var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

    if (user is null)
      return Result.Failure(ApplicationError.UserNotFoundError());

    var validationResult = _jwtService.ValidateToken(request.token);

    if (!validationResult)
    {
      return Result.Failure(ApplicationError.EmailConfirmationError(user.Email));
    }

    _jwtService.InvalidateToken(request.token);

    user.IsEmailConfirmed = true;
    await _userRepository.UpdateAsync(user, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}
