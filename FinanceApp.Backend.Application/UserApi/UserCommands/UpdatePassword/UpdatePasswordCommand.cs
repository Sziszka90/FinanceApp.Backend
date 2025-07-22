using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.UpdatePassword;

public record UpdatePasswordCommand(UpdatePasswordRequest UpdatePasswordDto, CancellationToken CancellationToken) : ICommand<Result>;
