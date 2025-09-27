using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.AuthApi.AuthCommands.ResetToken;

public record ResetTokenCommand(CancellationToken CancellationToken) : ICommand<Result>;


