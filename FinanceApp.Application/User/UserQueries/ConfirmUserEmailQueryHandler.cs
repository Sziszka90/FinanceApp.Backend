using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using FinanceApp.Application.User.UserQueries;

namespace FinanceApp.User.UserQueries;

public class ConfirmUserEmailQueryHandler : IQueryHandler<ConfirmUserEmailQuery, Result>
{
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IJwtService _jwtService;

  public ConfirmUserEmailQueryHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IJwtService jwtService)
  {
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
    _jwtService = jwtService;
  }

  public async Task<Result> Handle(ConfirmUserEmailQuery request, CancellationToken cancellationToken)
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

    user.IsEmailConfirmed = true;
    await _userRepository.UpdateAsync(user, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}
