using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TokenDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.UserApi.UserCommands.ValidateToken;

namespace FinanceApp.Backend.Application.AuthApi.AuthCommands.ValidateToken;

public record ValidateTokenCommand(ValidateTokenRequest validateTokenRequest, CancellationToken CancellationToken) : ICommand<Result<ValidateTokenResponse>>;


