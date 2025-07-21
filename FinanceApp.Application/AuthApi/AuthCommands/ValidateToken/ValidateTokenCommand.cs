using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using FinanceApp.Application.UserApi.UserCommands.ValidateToken;

namespace FinanceApp.Application.AuthApi.AuthCommands.ValidateToken;

public record ValidateTokenCommand(string Token, CancellationToken CancellationToken) : ICommand<Result<ValidateTokenResponse>>;


