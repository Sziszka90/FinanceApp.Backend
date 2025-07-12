using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.UserApi.UserCommands.ResetPassword;

public record ResetPasswordCommand(string token) : ICommand<Result>;
