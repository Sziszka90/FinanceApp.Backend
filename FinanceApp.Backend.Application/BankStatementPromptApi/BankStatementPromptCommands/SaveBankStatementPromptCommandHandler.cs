using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.BankStatementPromptDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.BankStatementPromptApi.BankStatementPromptCommands;

public class SaveBankStatementPromptCommandHandler : ICommandHandler<SaveBankStatementPromptCommand, Result<BankStatementPromptDto>>
{
  private readonly IRepository<BankStatementPrompt> _prompts;
  private readonly IUserService _users;
  private readonly IUnitOfWork _unitOfWork;

  public SaveBankStatementPromptCommandHandler(IRepository<BankStatementPrompt> prompts, IUserService users, IUnitOfWork unitOfWork)
  {
    _prompts = prompts;
    _users = users;
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<BankStatementPromptDto>> Handle(SaveBankStatementPromptCommand request, CancellationToken cancellationToken)
  {
    var user = await _users.GetActiveUserAsync(cancellationToken);
    if (!user.IsSuccess) return Result.Failure<BankStatementPromptDto>(user.ApplicationError!);

    BankStatementPrompt prompt;
    if (request.Id is Guid id)
    {
      var existingPrompt = await _prompts.GetSingleOrDefaultAsync(item => item.Id == id && item.UserId == user.Data!.Id,
        cancellationToken: cancellationToken);
      if (existingPrompt is null)
        return Result.Failure<BankStatementPromptDto>(ApplicationError.EntityNotFoundError(id.ToString(), nameof(BankStatementPrompt)));
      existingPrompt.Update(request.Prompt.Name.Trim(), request.Prompt.Instructions.Trim());
      prompt = existingPrompt;
    }
    else
    {
      prompt = new BankStatementPrompt
      {
        UserId = user.Data!.Id,
        User = user.Data,
        Name = request.Prompt.Name.Trim(),
        Instructions = request.Prompt.Instructions.Trim()
      };
      await _prompts.CreateAsync(prompt, cancellationToken);
    }

    await _unitOfWork.SaveChangesAsync(cancellationToken);
    return Result.Success(BankStatementPromptMapping.ToDto(prompt));
  }
}
