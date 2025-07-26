using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.AuthDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.AuthApi.AuthCommands.Login;

public record LoginCommand(LoginRequestDto LoginRequestDto, CancellationToken CancellationToken) : ICommand<Result<LoginResponseDto>>;
