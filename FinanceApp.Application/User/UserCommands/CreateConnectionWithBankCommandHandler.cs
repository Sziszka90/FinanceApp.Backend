using System.Security.Claims;
using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.SaltEdgeDtos;
using FinanceApp.Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using static FinanceApp.Application.Dtos.SaltEdgeDtos.CreateConnectionRequestDto;

namespace FinanceApp.Application.User.UserCommands;

public class CreateConnectionWithBankCommandHandler : ICommandHandler<CreateConnectionWithBankCommand, Result<CreateConnectionResponseDto>>
{
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly ILogger<CreateConnectionWithBankCommandHandler> _logger;
  private readonly ISaltEdgeClient _saltEdgeClient;
  private readonly IUserRepository _userRepository;

  public CreateConnectionWithBankCommandHandler(
                                  IHttpContextAccessor httpContextAccessor,
                                  ILogger<CreateConnectionWithBankCommandHandler> logger,
                                  ISaltEdgeClient saltEdgeClient,
                                  IUserRepository userRepository)
  {
    _logger = logger;
    _saltEdgeClient = saltEdgeClient;
    _httpContextAccessor = httpContextAccessor;
    _userRepository = userRepository;
  }

  /// <inheritdoc />
  public async Task<Result<CreateConnectionResponseDto>> Handle(CreateConnectionWithBankCommand request, CancellationToken cancellationToken)
  {
    var httpContext = _httpContextAccessor.HttpContext;

    var userEmail = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)
                                      ?.Value;

    if (userEmail is null)
    {
      _logger.LogError("User is not logged in");
      return Result.Failure<CreateConnectionResponseDto>(ApplicationError.UserNotFoundError());
    }

    var user = await _userRepository.GetUserByEmailAsync(userEmail!);

    var createConnectionDto = new CreateConnectionRequestDto
    {
      Data = new CreateConnectionDataRequestDto
      {
        CustomerId = user!.SaltEdgeIdentifier!,
        Consent = new ConsentDto
        {
          Scopes = ["holder_info", "accounts", "transactions"]
        },
        Attempt = new AttemptDto
        {
          ReturnTo = "www.financeapp.fun"
        }
      }
    };

    var createConnectionResult = await _saltEdgeClient.CreateConnectionAsync(createConnectionDto);

    if (!createConnectionResult.IsSuccess)
    {
      _logger.LogError("Failed to create connection with bank: {Error}", createConnectionResult.ApplicationError!.Message);
      return Result.Failure<CreateConnectionResponseDto>(createConnectionResult.ApplicationError);
    }

    return Result.Success<CreateConnectionResponseDto>(createConnectionResult.Data!);
  }
}
