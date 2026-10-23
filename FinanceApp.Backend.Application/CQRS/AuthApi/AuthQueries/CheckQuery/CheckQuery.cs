using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.AuthApi.AuthQueries.CheckQuery;

public record CheckQuery(CancellationToken CancellationToken) : IQuery<Result<bool>>;
