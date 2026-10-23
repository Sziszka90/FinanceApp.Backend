using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.UserApi.UserQueries.GetUserById;

public record GetUserByIdQuery(Guid Id, CancellationToken CancellationToken) : IQuery<Result<GetUserDto>>;
