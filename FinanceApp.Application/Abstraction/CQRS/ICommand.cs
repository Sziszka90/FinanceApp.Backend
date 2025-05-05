using FinanceApp.Application.Models;
using MediatR;

namespace FinanceApp.Application.Abstractions.CQRS;

public interface ICommand<out TResponse> : IRequest<TResponse> where TResponse : Result { };