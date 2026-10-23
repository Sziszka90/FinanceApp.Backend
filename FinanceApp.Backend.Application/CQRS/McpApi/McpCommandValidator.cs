using FinanceApp.Backend.Application.Dtos.McpDtos;
using FluentValidation;

namespace FinanceApp.Backend.Application.McpApi.McpCommands;

public class McpCommandValidator : AbstractValidator<McpCommand>
{
  public McpCommandValidator(IValidator<McpRequest> mcpRequestValidator)
  {
    RuleFor(x => x.McpRequest)
      .SetValidator(mcpRequestValidator);
  }
}
