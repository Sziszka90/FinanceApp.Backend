using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.ConfirmUserEmail;

public record ConfirmUserEmailCommand(Guid Id, string Token, CancellationToken CancellationToken) : ICommand<Result>;
