using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.UserApi.UserCommands.DeleteUser;

public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand, Result>
{
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.User> _userRepository;
  private readonly ILogger<DeleteUserCommandHandler> _logger;

  public DeleteUserCommandHandler(IMapper mapper,
                                  IUnitOfWork unitOfWork,
                                  IRepository<Domain.Entities.User> userRepository,
                                  ILogger<DeleteUserCommandHandler> logger)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _userRepository = userRepository;
    _logger = logger;
  }

  /// <inheritdoc />
  public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

    if (user is null)
    {
      _logger.LogError("User not found with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.UserNotFoundError(request.Id.ToString()));
    }

    await _userRepository.DeleteAsync(request.Id, cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("User deleted with ID:{Id}", user.Id);

    return Result.Success();
  }
}
