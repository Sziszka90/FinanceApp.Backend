using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.AuthApi.AuthCommands.Refresh;

public record RefreshCommand(string RefreshToken, CancellationToken CancellationToken) : ICommand<Result<string>>;


