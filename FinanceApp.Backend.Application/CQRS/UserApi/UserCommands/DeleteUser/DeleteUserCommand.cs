using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.DeleteUser;

public record DeleteUserCommand(Guid Id, CancellationToken CancellationToken) : ICommand<Result>;
