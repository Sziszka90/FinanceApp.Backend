using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.TransactionApi.TransactionQueries.GetTransactionById;

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
    _logger.LogDebug("Transaction retrieved with ID:{Id}", request.Id);
    return Result.Success(_mapper.Map<GetTransactionDto>(result));
  }
}
