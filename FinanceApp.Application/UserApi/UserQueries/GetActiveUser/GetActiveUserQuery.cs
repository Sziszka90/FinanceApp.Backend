using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.UserApi.UserQueries.GetActiveUser;

public record GetActiveUserQuery : IQuery<Result<GetUserDto>>;
