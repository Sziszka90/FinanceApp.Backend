using FinanceApp.Backend.Application.Models;
using MediatR;

namespace FinanceApp.Backend.Application.Abstractions.CQRS;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
  where TCommand : ICommand<TResponse> where TResponse : Result;
