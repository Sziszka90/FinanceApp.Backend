using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Saving.SavingCommands;

public record DeleteSavingCommand(Guid Id) : ICommand<Result>;