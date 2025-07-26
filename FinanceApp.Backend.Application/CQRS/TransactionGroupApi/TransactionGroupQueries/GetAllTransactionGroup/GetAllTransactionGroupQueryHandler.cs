using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupQueries.GetAllTransactionGroup;

public class GetAllTransactionGroupQueryHandler : IQueryHandler<GetAllTransactionGroupQuery, Result<List<GetTransactionGroupDto>>>
{
  private readonly ILogger<GetAllTransactionGroupQueryHandler> _logger;
  private readonly IMapper _mapper;
  private readonly IRepository<TransactionGroup> _transactionGroupRepository;

  public GetAllTransactionGroupQueryHandler(
    ILogger<GetAllTransactionGroupQueryHandler> logger,
    IMapper mapper,
    IRepository<TransactionGroup> transactionGroupRepository)
  {
    _logger = logger;
    _mapper = mapper;
    _transactionGroupRepository = transactionGroupRepository;
  }

  public async Task<Result<List<GetTransactionGroupDto>>> Handle(GetAllTransactionGroupQuery request, CancellationToken cancellationToken)
  {
    var result = await _transactionGroupRepository.GetAllAsync(noTracking: true, cancellationToken);
    _logger.LogInformation("Retrieved {Count} Transaction Groups", result.Count);
    return Result.Success(_mapper.Map<List<GetTransactionGroupDto>>(result));
  }
}
