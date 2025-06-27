using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.User.UserCommands;

public record ResetPasswordCommand(string token) : ICommand<Result>;
