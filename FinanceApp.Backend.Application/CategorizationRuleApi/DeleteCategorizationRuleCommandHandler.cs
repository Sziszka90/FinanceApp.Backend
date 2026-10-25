using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.CategorizationRuleApi;

public class DeleteCategorizationRuleCommandHandler : ICommandHandler<DeleteCategorizationRuleCommand, Result<bool>>
{
  private readonly IRepository<CategorizationRule> _rules;
  private readonly IUserService _users;
  private readonly IUnitOfWork _unitOfWork;

  public DeleteCategorizationRuleCommandHandler(IRepository<CategorizationRule> rules, IUserService users, IUnitOfWork unitOfWork)
  {
    _rules = rules;
    _users = users;
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<bool>> Handle(DeleteCategorizationRuleCommand request, CancellationToken cancellationToken)
  {
    var user = await _users.GetActiveUserAsync(cancellationToken);
    if (!user.IsSuccess) return Result.Failure<bool>(user.ApplicationError!);

    var rule = await _rules.GetSingleOrDefaultAsync(item => item.Id == request.Id && item.UserId == user.Data!.Id,
      cancellationToken: cancellationToken);
    if (rule is null) return Result.Failure<bool>(ApplicationError.EntityNotFoundError(request.Id.ToString(), nameof(CategorizationRule)));

    _rules.Delete(rule);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    return Result.Success(true);
  }
}
