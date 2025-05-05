using MediatR;

namespace FinanceApp.Application.Abstractions.CQRS;

public interface IQuery<out TResponse> : IRequest<TResponse> { }