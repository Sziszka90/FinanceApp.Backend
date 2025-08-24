using System.Data;
using FinanceApp.Backend.Application.Dtos.McpDtos;
using FluentValidation;

namespace FinanceApp.Backend.Application.Validators;

public class McpRequestValidator : AbstractValidator<McpRequest>
{
  public McpRequestValidator()
  {
    RuleFor(x => x.Arguments)
      .NotEmpty()
      .WithMessage("Arguments cannot be empty.");

    RuleFor(x => x.Name)
      .NotEmpty()
      .WithMessage("Name cannot be empty.");

    RuleFor(x => x.Arguments)
      .Must(args => args is not null && args.Count > 0)
      .WithMessage("Arguments must contain at least one entry.");

    RuleFor(x => x.Arguments)
      .Must(args => args != null && args.TryGetValue("startDate", out var value) && value is DateTime)
      .WithMessage("Argument 'startDate' is required and must be of type DateTime.");

    RuleFor(x => x.Arguments)
      .Must(args => args != null && args.TryGetValue("endDate", out var value) && value is DateTime)
      .WithMessage("Argument 'endDate' is required and must be of type DateTime.");

    RuleFor(x => x.Arguments)
      .Must(args => args != null && args.TryGetValue("top", out var value) && value is int)
      .WithMessage("Argument 'top' is required and must be of type int.");

    RuleFor(x => x.Arguments)
      .Must(args => args != null && args.TryGetValue("userId", out var value) && value is Guid)
      .WithMessage("Argument 'userId' is required and must be of type Guid.");
  }
}
