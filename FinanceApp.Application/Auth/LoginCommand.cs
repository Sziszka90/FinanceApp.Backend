using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.AuthDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Auth;

public record LoginCommand(LoginRequestDto LoginRequestDto) : ICommand<Result<LoginResponseDto>>;