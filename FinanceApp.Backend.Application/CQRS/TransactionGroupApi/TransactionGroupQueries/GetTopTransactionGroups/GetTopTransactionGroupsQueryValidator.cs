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
        .WithMessage("End date must be greater than or equal to start date.")
        .LessThanOrEqualTo(DateTimeOffset.UtcNow)
        .WithMessage("End date cannot be in the future.");

    RuleFor(x => x.Top)
        .GreaterThan(0)
        .WithMessage("Top must be greater than 0.")
        .LessThanOrEqualTo(100)
        .WithMessage("Top cannot exceed 100 items for performance reasons.");

    RuleFor(x => x)
        .Must(x => (x.EndDate - x.StartDate).TotalDays <= 365)
        .WithMessage("Date range cannot exceed 365 days for performance reasons.")
        .When(x => x.StartDate != default && x.EndDate != default);

    RuleFor(x => x)
        .Must(x => (x.EndDate - x.StartDate).TotalDays >= 0)
        .WithMessage("Invalid date range.")
        .When(x => x.StartDate != default && x.EndDate != default);
  }
}
