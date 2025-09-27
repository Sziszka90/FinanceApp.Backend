using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.AuthApi.AuthCommands.Logout;

public record LogoutCommand(string? Token, string? RefreshToken, CancellationToken CancellationToken) : ICommand<Result>;
