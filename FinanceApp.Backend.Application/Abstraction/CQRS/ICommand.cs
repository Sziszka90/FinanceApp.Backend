using FinanceApp.Backend.Application.Models;
using MediatR;

namespace FinanceApp.Backend.Application.Abstractions.CQRS;

public interface ICommand<out TResponse> : IRequest<TResponse> where TResponse : Result { };
