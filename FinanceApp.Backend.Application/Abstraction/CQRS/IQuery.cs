using MediatR;

namespace FinanceApp.Backend.Application.Abstractions.CQRS;

public interface IQuery<out TResponse> : IRequest<TResponse> { }
