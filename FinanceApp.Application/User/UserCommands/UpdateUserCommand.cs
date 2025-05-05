using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.User.UserCommands;

public record UpdateUserCommand(UpdateUserDto UpdateUserDto) : ICommand<Result<GetUserDto>>;