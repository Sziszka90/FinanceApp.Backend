using FinanceApp.Application.Models;
using MediatR;

namespace FinanceApp.Application.Abstractions.CQRS;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
  where TCommand : ICommand<TResponse> where TResponse : Result;
