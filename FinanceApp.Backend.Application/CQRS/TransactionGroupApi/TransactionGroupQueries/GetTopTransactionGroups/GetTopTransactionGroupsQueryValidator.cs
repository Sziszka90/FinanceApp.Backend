using FluentValidation;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupQueries.GetTopTransactionGroups;

public class GetTopTransactionGroupsQueryValidator : AbstractValidator<GetTopTransactionGroupsQuery>
{
  public GetTopTransactionGroupsQueryValidator()
  {
    RuleFor(x => x.StartDate)
        .NotEmpty()
        .WithMessage("Start date is required.")
        .LessThanOrEqualTo(DateTimeOffset.UtcNow)
        .WithMessage("Start date cannot be in the future.");

    RuleFor(x => x.EndDate)
        .NotEmpty()
        .WithMessage("End date is required.")
        .GreaterThanOrEqualTo(x => x.StartDate)
        .WithMessage("End date must be greater than or equal to start date.");

    RuleFor(x => x.Top)
        .GreaterThan(0)
        .WithMessage("Top must be greater than 0.")
        .LessThanOrEqualTo(100)
        .WithMessage("Top cannot exceed 100 items for performance reasons.");
  }
}
