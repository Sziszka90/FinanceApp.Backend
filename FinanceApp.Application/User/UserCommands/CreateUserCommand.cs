using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.User.UserCommands;

public record CreateUserCommand(CreateUserDto CreateUserDto) : ICommand<Result<GetUserDto>>;
