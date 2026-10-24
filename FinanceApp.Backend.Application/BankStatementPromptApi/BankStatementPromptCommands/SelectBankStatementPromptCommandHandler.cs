using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.BankStatementPromptApi.BankStatementPromptCommands;

public class SelectBankStatementPromptCommandHandler : ICommandHandler<SelectBankStatementPromptCommand, Result<bool>>
{
  private readonly IRepository<BankStatementPrompt> _prompts;
  private readonly IUserService _users;
  private readonly IUnitOfWork _unitOfWork;

  public SelectBankStatementPromptCommandHandler(IRepository<BankStatementPrompt> prompts, IUserService users, IUnitOfWork unitOfWork)
  {
    _prompts = prompts;
    _users = users;
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<bool>> Handle(SelectBankStatementPromptCommand request, CancellationToken cancellationToken)
  {
    var user = await _users.GetActiveUserAsync(cancellationToken);
    if (!user.IsSuccess)
    {
      return Result.Failure<bool>(user.ApplicationError!);
    }

    var prompts = await _prompts.GetAllAsync(prompt => prompt.UserId == user.Data!.Id, cancellationToken: cancellationToken);
    if (request.PromptId.HasValue && prompts.All(prompt => prompt.Id != request.PromptId.Value))
    {
      return Result.Failure<bool>(ApplicationError.EntityNotFoundError(request.PromptId.Value.ToString(), nameof(BankStatementPrompt)));
    }

    foreach (var prompt in prompts)
    {
      prompt.IsSelected = prompt.Id == request.PromptId;
    }

    await _unitOfWork.SaveChangesAsync(cancellationToken);
    return Result.Success(true);
  }
}
