using System.Security.Claims;
using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Investment.InvestmentCommands;

public class CreateInvestmentCommandHandler : ICommandHandler<CreateInvestmentCommand, Result<GetInvestmentDto>>
{
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.Investment> _investmentRepository;
  private readonly ILogger<CreateInvestmentCommandHandler> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IUserRepository _userRepository;

  public CreateInvestmentCommandHandler(IMapper mapper,
                                        IUnitOfWork unitOfWork,
                                        IRepository<Domain.Entities.Investment> investmentRepository,
                                        ILogger<CreateInvestmentCommandHandler> logger,
                                        IUserRepository userRepository,
                                        IHttpContextAccessor httpContextAccessor)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _investmentRepository = investmentRepository;
    _logger = logger;
    _userRepository = userRepository;
    _httpContextAccessor = httpContextAccessor;
  }

  /// <inheritdoc />
  public async Task<Result<GetInvestmentDto>> Handle(CreateInvestmentCommand request, CancellationToken cancellationToken)
  {
    var criteria = InvestmentQueryCriteria.FindDuplicatedName(request.CreateInvestmentDto);

    var investmentWithSameName = await _investmentRepository.GetQueryAsync(criteria, cancellationToken: cancellationToken);

    if (investmentWithSameName.Count > 0)
    {
      _logger.LogError("Investment already exists with name:{Name}", request.CreateInvestmentDto.Name);
      return Result.Failure<GetInvestmentDto>(ApplicationError.NameAlreadyExistsError(request.CreateInvestmentDto.Name));
    }

    var httpContext = _httpContextAccessor.HttpContext;

    var userEmail = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)
                                      ?.Value;

    if (userEmail is null)
    {
      _logger.LogError("User is not logged in");
      return Result.Failure<GetInvestmentDto>(ApplicationError.UserNotFoundError());
    }

    var user = await _userRepository.GetUserByEmailAsync(userEmail!);

    var investment = await _investmentRepository.CreateAsync(new Domain.Entities.Investment(
                                                               request.CreateInvestmentDto.Name,
                                                               request.CreateInvestmentDto.Value,
                                                               request.CreateInvestmentDto.Description,
                                                               user!), cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);
    _logger.LogInformation("Investment created with ID:{Id}", investment.Id);

    return Result.Success(_mapper.Map<GetInvestmentDto>(investment));
  }
}
