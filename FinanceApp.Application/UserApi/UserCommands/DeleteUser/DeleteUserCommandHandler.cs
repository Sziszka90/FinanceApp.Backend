using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.UserApi.UserCommands.DeleteUser;

public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand, Result>
{

  private readonly ILogger<DeleteUserCommandHandler> _logger;
  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.User> _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ITransactionRepository _transactionRepository;
  private readonly ITransactionGroupRepository _transactionGroupRepository;

  public DeleteUserCommandHandler(
    ILogger<DeleteUserCommandHandler> logger,
    IRepository<Domain.Entities.User> userRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ITransactionRepository transactionRepository,
    ITransactionGroupRepository transactionGroupRepository
  )
  {
    _logger = logger;
    _mapper = mapper;
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
    _transactionRepository = transactionRepository;
    _transactionGroupRepository = transactionGroupRepository;
  }

  /// <inheritdoc />
  public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByIdAsync(request.Id, noTracking: false, cancellationToken: cancellationToken);

    if (user is null)
    {
      _logger.LogError("User not found with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.UserNotFoundError(request.Id.ToString()));
    }

    await _transactionRepository.DeleteAllByUserIdAsync(user.Id, cancellationToken);
    await _transactionGroupRepository.DeleteAllByUserIdAsync(user.Id, cancellationToken);

    await _userRepository.DeleteAsync(user, cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogDebug("User deleted with ID:{Id}", user.Id);
    _logger.LogDebug("User related transactions and groups deleted for user with ID:{Id}", user.Id);

    return Result.Success();
  }
}
