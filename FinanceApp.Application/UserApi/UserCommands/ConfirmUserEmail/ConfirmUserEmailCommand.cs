using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.UserApi.UserCommands.ConfirmUserEmail;

public record ConfirmUserEmailCommand(Guid Id, string token) : ICommand<Result>;
