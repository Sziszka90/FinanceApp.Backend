using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.BankStatementPromptDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.BankStatementPromptApi.BankStatementPromptCommands;

public record SaveBankStatementPromptCommand(SaveBankStatementPromptDto Prompt, Guid? Id = null) : ICommand<Result<BankStatementPromptDto>>;
