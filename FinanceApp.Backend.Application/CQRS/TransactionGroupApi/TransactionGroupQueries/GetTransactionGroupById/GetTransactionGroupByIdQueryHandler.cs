using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupQueries.GetTransactionGroupById;

public class GetTransactionGroupByIdQueryHandler : IQueryHandler<GetTransactionGroupByIdQuery, Result<GetTransactionGroupDto>>
{
  private readonly ILogger<GetTransactionGroupByIdQueryHandler> _logger;
  private readonly IMapper _mapper;
  private readonly ITransactionGroupRepository _transactionGroupRepository;

  public GetTransactionGroupByIdQueryHandler(
    ILogger<GetTransactionGroupByIdQueryHandler> logger,
    IMapper mapper,
    ITransactionGroupRepository transactionGroupRepository)
  {
    _logger = logger;
    _mapper = mapper;
    _transactionGroupRepository = transactionGroupRepository;
  }

  public async Task<Result<GetTransactionGroupDto>> Handle(GetTransactionGroupByIdQuery request, CancellationToken cancellationToken)
  {
    var result = await _transactionGroupRepository.GetByIdAsync(request.Id, noTracking: true, cancellationToken: cancellationToken);

    if (result is null)
    {
      _logger.LogWarning("Transaction Group not found with ID:{Id}", request.Id);
      return Result.Failure<GetTransactionGroupDto>(ApplicationError.EntityNotFoundError(request.Id.ToString()));
    }

    _logger.LogInformation("Retrieved Transaction Group with ID:{Id}", request.Id);
    return Result.Success(_mapper.Map<GetTransactionGroupDto>(result));
  }
}
