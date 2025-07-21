using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.UserApi.UserCommands.UpdatePassword;

public record UpdatePasswordCommand(UpdatePasswordRequest UpdatePasswordDto, CancellationToken CancellationToken) : ICommand<Result>;
