using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.BankStatementPromptDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.BankStatementPromptApi.BankStatementPromptQueries;

public class GetBankStatementPromptsQueryHandler : IQueryHandler<GetBankStatementPromptsQuery, Result<List<BankStatementPromptDto>>>
{
  private readonly IRepository<BankStatementPrompt> _prompts;
  private readonly IUserService _users;

  public GetBankStatementPromptsQueryHandler(IRepository<BankStatementPrompt> prompts, IUserService users)
  {
    _prompts = prompts;
    _users = users;
  }

  public async Task<Result<List<BankStatementPromptDto>>> Handle(GetBankStatementPromptsQuery request, CancellationToken cancellationToken)
  {
    var user = await _users.GetActiveUserAsync(cancellationToken);
    if (!user.IsSuccess)
    {
      return Result.Failure<List<BankStatementPromptDto>>(user.ApplicationError!);
    }

    var prompts = await _prompts.GetAllAsync(prompt => prompt.UserId == user.Data!.Id,
      noTracking: true, cancellationToken: cancellationToken);
    return Result.Success(prompts.Select(BankStatementPromptMapping.ToDto).OrderBy(prompt => prompt.Name).ToList());
  }
}
