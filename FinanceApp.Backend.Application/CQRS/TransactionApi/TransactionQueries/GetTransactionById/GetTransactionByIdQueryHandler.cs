using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetTransactionById;

public class GetTransactionByIdQueryHandler : IQueryHandler<GetTransactionByIdQuery, Result<GetTransactionDto>>
{
  private readonly ILogger<GetTransactionByIdQueryHandler> _logger;
  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.Transaction> _transactionRepository;

  public GetTransactionByIdQueryHandler(
    ILogger<GetTransactionByIdQueryHandler> logger,
    IMapper mapper,
    IRepository<Domain.Entities.Transaction> transactionRepository)
  {
    _logger = logger;
    _mapper = mapper;
    _transactionRepository = transactionRepository;
  }

  public async Task<Result<GetTransactionDto>> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
  {
    var result = await _transactionRepository.GetByIdAsync(request.Id, noTracking: true, cancellationToken: cancellationToken);

    if (result is null)
    {
      _logger.LogError("Transaction with ID:{Id} not found", request.Id);
      return Result.Failure<GetTransactionDto>(ApplicationError.EntityNotFoundError(request.Id.ToString(), nameof(Domain.Entities.Transaction)));
    }

    _logger.LogInformation("Transaction retrieved with ID:{Id}", request.Id);
    return Result.Success(_mapper.Map<GetTransactionDto>(result));
  }
}
