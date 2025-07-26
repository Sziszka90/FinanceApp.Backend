using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.CreateUser;

public record CreateUserCommand(CreateUserDto CreateUserDto, CancellationToken CancellationToken) : ICommand<Result<GetUserDto>>;
