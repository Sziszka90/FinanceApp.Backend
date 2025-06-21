using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.User.UserQueries;

public record ConfirmUserEmailQuery(Guid Id, string token) : IQuery<Result<GetUserDto>>;
