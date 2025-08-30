using FinanceApp.Backend.Application.Dtos.McpDtos;
using FinanceApp.Backend.Application.Models;
using FluentValidation;

namespace FinanceApp.Backend.Application.Validators;

public class McpRequestValidator : AbstractValidator<McpRequest>
{
  private static bool IsConvertibleToDateTimeOffset(object? value)
  {
    if (value is DateTimeOffset || value is DateTime)
    {
      return true;
    }

    if (value is string s && DateTimeOffset.TryParse(s, out _))
    {
      return true;
    }

    if (value is System.Text.Json.JsonElement json)
    {
      if (json.ValueKind == System.Text.Json.JsonValueKind.String &&
          DateTimeOffset.TryParse(json.GetString(), out _))
      {
        return true;
      }
    }

    return false;
  }

  private static bool IsConvertibleToInt(object? value)
  {
    if (value is int)
    {
      return true;
    }

    if (value is string s && int.TryParse(s, out _))
    {
      return true;
    }

    if (value is System.Text.Json.JsonElement json)
    {
      if (json.ValueKind == System.Text.Json.JsonValueKind.Number && json.TryGetInt32(out _))
      {
        return true;
      }

      if (json.ValueKind == System.Text.Json.JsonValueKind.String && int.TryParse(json.GetString(), out _))
      {
        return true;
      }
    }

    return false;
  }

  private static bool IsConvertibleToGuid(object? value)
  {
    if (value is Guid)
    {
      return true;
    }

    if (value is string s && Guid.TryParse(s, out _))
    {
      return true;
    }

    if (value is System.Text.Json.JsonElement json)
    {
      if (json.ValueKind == System.Text.Json.JsonValueKind.String && Guid.TryParse(json.GetString(), out _))
      {
        return true;
      }
    }

    return false;
  }

  public McpRequestValidator()
  {
    RuleFor(x => x.Parameters)
      .NotEmpty()
      .WithMessage("Parameters cannot be empty.");

    RuleFor(x => x.Action)
      .NotEmpty()
      .WithMessage("Action cannot be empty.")
      .Must(action => SupportedTools.SupportedActions.Contains(action))
      .WithMessage("Action must be one of the supported tools.");

    RuleFor(x => x.Parameters)
      .Must(parameters => parameters != null && parameters.TryGetValue("user_id", out var value) && IsConvertibleToGuid(value))
      .WithMessage("Parameter 'user_id' is required and must be of type Guid.");

    RuleFor(x => x.Parameters)
      .Must(parameters => parameters != null && parameters.TryGetValue("correlation_id", out var value) && IsConvertibleToGuid(value))
      .WithMessage("Parameter 'correlation_id' is required and must be of type Guid.");

    RuleFor(x => x.Parameters)
      .Must(parameters => parameters.TryGetValue("start_date", out var value) && IsConvertibleToDateTimeOffset(value))
      .When(x => x.Parameters != null && x.Parameters.ContainsKey("start_date"))
      .WithMessage("Parameter 'start_date' must be convertible to DateTimeOffset.");

    RuleFor(x => x.Parameters)
      .Must(parameters => parameters.TryGetValue("end_date", out var value) && IsConvertibleToDateTimeOffset(value))
      .When(x => x.Parameters != null && x.Parameters.ContainsKey("end_date"))
      .WithMessage("Parameter 'end_date' must be convertible to DateTimeOffset.");

    RuleFor(x => x.Parameters)
      .Must(parameters => parameters.TryGetValue("top", out var value) && IsConvertibleToInt(value))
      .When(x => x.Parameters != null && x.Parameters.ContainsKey("top"))
      .WithMessage("Parameter 'top' must be of type int.");
  }
}
