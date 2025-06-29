using System.Security.Claims;
using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Saving.SavingCommands;

public class CreateSavingCommandHandler : ICommandHandler<CreateSavingCommand, Result<GetSavingDto>>
{
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.Saving> _savingRepository;
  private readonly ILogger<CreateSavingCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IUserRepository _userRepository;

  public CreateSavingCommandHandler(IMapper mapper,
                                    IUnitOfWork unitOfWork,
                                    IRepository<Domain.Entities.Saving> savingRepository,
                                    ILogger<CreateSavingCommandHandler> logger,
                                    IUserRepository userRepository,
                                    IHttpContextAccessor httpContextAccessor)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _savingRepository = savingRepository;
    _logger = logger;
    _userRepository = userRepository;
    _httpContextAccessor = httpContextAccessor;
  }

  /// <inheritdoc />
  public async Task<Result<GetSavingDto>> Handle(CreateSavingCommand request, CancellationToken cancellationToken)
  {
    var criteria = SavingQueryCriteria.FindDuplicatedName(request.CreateSavingDto);

    var savingWithSameName = await _savingRepository.GetQueryAsync(criteria, cancellationToken: cancellationToken);

    if (savingWithSameName.Count > 0)
    {
      _logger.LogError("Saving already exists with name:{Name}", request.CreateSavingDto.Name);
      return Result.Failure<GetSavingDto>(ApplicationError.NameAlreadyExistsError(request.CreateSavingDto.Name));
    }

    var httpContext = _httpContextAccessor.HttpContext;

    var userEmail = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)
                                      ?.Value;

    if (userEmail is null)
    {
      _logger.LogError("User is not logged in");
      return Result.Failure<GetSavingDto>(ApplicationError.UserNotFoundError());
    }

    var user = await _userRepository.GetUserByEmailAsync(userEmail!);

    var saving = await _savingRepository.CreateAsync(new Domain.Entities.Saving(
                                                       request.CreateSavingDto.Name,
                                                       request.CreateSavingDto.Description,
                                                       request.CreateSavingDto.Value,
                                                       request.CreateSavingDto.Type,
                                                       request.CreateSavingDto.DueDate,
                                                       user!), cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Saving created with ID:{Id}", saving.Id);

    return Result.Success(_mapper.Map<GetSavingDto>(saving));
  }
}
