using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.UserApi.UserQueries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IQuery<Result<GetUserDto>>;
