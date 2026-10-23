using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.UserApi.UserQueries.GetActiveUser;

public record GetActiveUserQuery(CancellationToken CancellationToken) : IQuery<Result<GetUserDto>>;
