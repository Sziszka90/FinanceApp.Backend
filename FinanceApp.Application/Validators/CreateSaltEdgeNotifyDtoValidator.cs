using FluentValidation;
using FinanceApp.Application.Dtos.SaltEdgeDtos;

namespace FinanceApp.Application.Validators;

public class CreateSaltEdgeNotifyDtoValidator : AbstractValidator<CreateSaltEdgeNotifyRequestDto>
{
  public CreateSaltEdgeNotifyDtoValidator()
  {
    RuleFor(x => x.Data).NotNull();
    RuleFor(x => x.Data.CustomerId).NotEmpty();
    RuleFor(x => x.Data.ConnectionId).NotEmpty();
    RuleFor(x => x.Data.Status).NotEmpty();
    RuleFor(x => x.Data.AttemptId).NotEmpty();
    RuleFor(x => x.Data.CustomFields).NotNull();
    RuleFor(x => x.Data.CustomFields.UserId).NotEmpty();
  }
}
