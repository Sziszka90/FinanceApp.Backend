using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.UserApi.UserCommands.DeleteUser;

public record DeleteUserCommand(Guid Id, CancellationToken CancellationToken) : ICommand<Result>;
