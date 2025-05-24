using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Investment.InvestmentCommands;

public class UpdateInvestmentCommandHandler : ICommandHandler<UpdateInvestmentCommand, Result<GetInvestmentDto>>
{
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.Investment> _investmentRepository;
  private readonly ILogger<UpdateInvestmentCommandHandler> _logger;

  public UpdateInvestmentCommandHandler(IMapper mapper,
                                        IUnitOfWork unitOfWork,
                                        IRepository<Domain.Entities.Investment> investmentRepository,
                                        ILogger<UpdateInvestmentCommandHandler> logger)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _investmentRepository = investmentRepository;
    _logger = logger;
  }

  /// <inheritdoc />
  public async Task<Result<GetInvestmentDto>> Handle(UpdateInvestmentCommand request, CancellationToken cancellationToken)
  {
    var investmentWithSameName = await _investmentRepository.GetQueryAsync(InvestmentQueryCriteria.FindDuplicatedNameExludingId(request.UpdateInvestmentDto), cancellationToken: cancellationToken);

    if (investmentWithSameName.Count > 0)
    {
      _logger.LogError("Investment already exists with name:{Name} ", request.UpdateInvestmentDto.Name);
      return Result.Failure<GetInvestmentDto>(ApplicationError.NameAlreadyExistsError(request.UpdateInvestmentDto.Name));
    }

    var investment = await _investmentRepository.GetByIdAsync(request.UpdateInvestmentDto.Id, cancellationToken);

    if (investment is null)
    {
      _logger.LogError("Investment not found with ID:{Id}", request.UpdateInvestmentDto.Id);
      return Result.Failure<GetInvestmentDto>(ApplicationError.EntityNotFoundError());
    }

    investment.Update(
      request.UpdateInvestmentDto.Name,
      request.UpdateInvestmentDto.Value,
      request.UpdateInvestmentDto.Description
    );

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Investment updated with ID:{Id}", request.UpdateInvestmentDto.Id);

    return Result.Success(_mapper.Map<GetInvestmentDto>(await _investmentRepository.UpdateAsync(investment, cancellationToken)));
  }
}
