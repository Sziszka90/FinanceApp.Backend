using MediatR;

namespace FinanceApp.Backend.Application.Abstractions.CQRS;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
  where TQuery : IQuery<TResponse>
{ }
