using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.User.UserCommands;

public record ConfirmUserEmailCommand(Guid Id, string token) : ICommand<Result>;
