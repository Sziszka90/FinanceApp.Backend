using FluentValidation;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetMonthlyTransactionContext;

public sealed class GetMonthlyTransactionContextQueryValidator : AbstractValidator<GetMonthlyTransactionContextQuery>
{
  public GetMonthlyTransactionContextQueryValidator()
  {
    RuleFor(query => query.Year)
      .InclusiveBetween(2000, 2100);

    RuleFor(query => query.Month)
      .InclusiveBetween(1, 12);

    RuleFor(query => query.LookbackMonths)
      .InclusiveBetween(1, 12);
  }
}
