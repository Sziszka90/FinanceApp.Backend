using FinanceApp.Application.Dtos.SaltEdgeDtos;
using FluentValidation;

namespace FinanceApp.Application.SaltEdge.SaltEdgeCommands;

public class CreateSaltEdgeNotifyCommandValidator : AbstractValidator<CreateSaltEdgeNotifyCommand>
{
  public CreateSaltEdgeNotifyCommandValidator(IValidator<CreateSaltEdgeNotifyRequestDto> createSaltEdgeNotifyDto)
  {
    RuleFor(x => x.CreateSaltEdgeNotifyDto)
      .SetValidator(createSaltEdgeNotifyDto);
  }
}
