using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.UpdateUser;

public record UpdateUserCommand(UpdateUserRequest UpdateUserDto, CancellationToken CancellationToken) : ICommand<Result<GetUserDto>>;
