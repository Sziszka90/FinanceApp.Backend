using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.UserApi.UserCommands.UpdateUser;

public record UpdateUserCommand(UpdateUserDto UpdateUserDto, CancellationToken CancellationToken) : ICommand<Result<GetUserDto>>;
