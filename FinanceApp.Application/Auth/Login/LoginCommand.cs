using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.AuthDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.AuthApi.Login;

public record LoginCommand(LoginRequestDto LoginRequestDto) : ICommand<Result<LoginResponseDto>>;
