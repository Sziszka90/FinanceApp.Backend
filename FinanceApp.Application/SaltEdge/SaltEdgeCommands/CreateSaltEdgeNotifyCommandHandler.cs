using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.SaltEdge.SaltEdgeCommands;

public class CreateSaltEdgeNotifyCommandHandler : ICommandHandler<CreateSaltEdgeNotifyCommand, Result>
{
  private readonly ILogger<CreateSaltEdgeNotifyCommandHandler> _logger;
  private readonly ISaltEdgeClient _saltEdgeClient;
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;

  public CreateSaltEdgeNotifyCommandHandler(
    ILogger<CreateSaltEdgeNotifyCommandHandler> logger,
    ISaltEdgeClient saltEdgeClient,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
  {
    _logger = logger;
    _saltEdgeClient = saltEdgeClient;
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<Result> Handle(CreateSaltEdgeNotifyCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetUserByEmailAsync(request.CreateSaltEdgeNotifyDto.Data.CustomFields.UserId);

    if (user is null)
    {
      _logger.LogError("User with email {Email} not found", request.CreateSaltEdgeNotifyDto.Data.CustomFields.UserId);
      return Result.Failure(ApplicationError.UserNotFoundError());
    }

    if (user.SaltEdgeIdentifier != request.CreateSaltEdgeNotifyDto.Data.CustomerId)
    {
      _logger.LogError("User with email {Email} does not have SaltEdge identifier {SaltEdgeIdentifier}",
        request.CreateSaltEdgeNotifyDto.Data.CustomFields.UserId, request.CreateSaltEdgeNotifyDto.Data.CustomerId);
      return Result.Failure(ApplicationError.MissingSaltEdgeIdentifierError(request.CreateSaltEdgeNotifyDto.Data.CustomFields.UserId));
    }




  }
}
