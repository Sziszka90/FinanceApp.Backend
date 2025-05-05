using FinanceApp.Application.Models;
using FluentValidation;
using MediatR;

namespace FinanceApp.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
  where TResponse : Result, new()
{
  #region Members

  private readonly IEnumerable<IValidator<TRequest>> _validators;

  #endregion

  #region Constructors

  public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
  {
    _validators = validators;
  }

  #endregion

  #region Methods

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    var context = new ValidationContext<TRequest>(request);

    var failures = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
    var errorList = failures.SelectMany(result => result.Errors)
                            .Where(f => f != null)
                            .ToList();

    if (errorList.Count is 0)
    {
      return await next();
    }

    var response = new TResponse
    {
      ApplicationError = ApplicationError.ValidationError(errorList)
    };

    return response;
  }

  #endregion
}