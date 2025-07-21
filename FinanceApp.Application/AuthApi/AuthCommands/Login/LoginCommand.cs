using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.AuthDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.AuthApi.AuthCommands.Login;

public record LoginCommand(LoginRequestDto LoginRequestDto, CancellationToken CancellationToken) : ICommand<Result<LoginResponseDto>>;
