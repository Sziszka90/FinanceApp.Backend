using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.BankStatementPromptApi.BankStatementPromptCommands;

public record SelectBankStatementPromptCommand(Guid? PromptId) : ICommand<Result<bool>>;
