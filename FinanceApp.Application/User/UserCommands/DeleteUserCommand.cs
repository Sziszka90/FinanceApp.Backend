using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.User.UserCommands;

public record DeleteUserCommand(Guid Id) : ICommand<Result>;