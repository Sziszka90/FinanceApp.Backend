using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.BankStatementPromptApi.BankStatementPromptCommands;

public class DeleteBankStatementPromptCommandHandler : ICommandHandler<DeleteBankStatementPromptCommand, Result<bool>>
{
  private readonly IRepository<BankStatementPrompt> _prompts;
  private readonly IUserService _users;
  private readonly IUnitOfWork _unitOfWork;

  public DeleteBankStatementPromptCommandHandler(IRepository<BankStatementPrompt> prompts, IUserService users, IUnitOfWork unitOfWork)
  {
    _prompts = prompts;
    _users = users;
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<bool>> Handle(DeleteBankStatementPromptCommand request, CancellationToken cancellationToken)
  {
    var user = await _users.GetActiveUserAsync(cancellationToken);
    if (!user.IsSuccess) return Result.Failure<bool>(user.ApplicationError!);

    var prompt = await _prompts.GetSingleOrDefaultAsync(item => item.Id == request.Id && item.UserId == user.Data!.Id,
      cancellationToken: cancellationToken);
    if (prompt is null)
      return Result.Failure<bool>(ApplicationError.EntityNotFoundError(request.Id.ToString(), nameof(BankStatementPrompt)));

    _prompts.Delete(prompt);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    return Result.Success(true);
  }
}
