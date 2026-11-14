using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FinanceApp.Backend.Application.Dtos.McpDtos;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetMonthlyTransactionContext;
using FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupQueries.GetTopTransactionGroups;
using MediatR;
using ModelContextProtocol.Server;

namespace FinanceApp.Backend.Presentation.WebApi.Mcp;

[McpServerToolType]
public sealed class FinanceMcpTools
{
  private const int MAX_LOOPBACK_MONTHS = 12;
  private const int MAX_YEAR = 2100;

  private readonly IMediator _mediator;
  private readonly ILogger<FinanceMcpTools> _logger;

  public FinanceMcpTools(
    IMediator mediator,
    ILogger<FinanceMcpTools> logger)
  {
    _mediator = mediator;
    _logger = logger;
  }

  [McpServerTool(
    Name = "get_top_transaction_groups",
    Title = "Get Top Transaction Groups",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    UseStructuredContent = true)]
  [Description("Returns the user's transaction group totals for an inclusive date range, converted to the user's base currency.")]
  public async Task<List<TopTransactionGroupDto>> GetTopTransactionGroups(
    [Description("Start of the transaction date range, inclusive.")] DateTimeOffset startDate,
    [Description("End of the transaction date range, inclusive.")] DateTimeOffset endDate,
    [Range(1, 20), Description("Maximum number of groups to return. Choose between 1 and 20.")] int top = 10,
    CancellationToken cancellationToken = default)
  {
    if (startDate > endDate)
    {
      throw new ArgumentException("startDate must be earlier than or equal to endDate.");
    }

    if (top is < 1 or > 20)
    {
      throw new ArgumentOutOfRangeException(nameof(top), "top must be between 1 and 20.");
    }

    var result = await _mediator.Send(
      new GetTopTransactionGroupsQuery(startDate, endDate, UserId: null, Top: top),
      cancellationToken);

    if (!result.IsSuccess || result.Data is null)
    {
      _logger.LogWarning("Could not retrieve top transaction groups for the authenticated user: {Error}", result.ApplicationError?.Message);
      throw new InvalidOperationException("Could not retrieve transaction groups for the authenticated user.");
    }

    return result.Data;
  }

  [McpServerTool(
    Name = "get_monthly_transaction_context",
    Title = "Get Monthly Transaction Context",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    UseStructuredContent = true)]
  [Description("Returns the authenticated user's transactions for the selected month and the requested preceding months, converted to the user's base currency. Use this context to compare spending, spot unusually large transactions, identify recurring payments, and compare income with expenses.")]
  public async Task<MonthlyTransactionContext> GetMonthlyTransactionContext(
    [Range(2000, MAX_YEAR), Description("Calendar year of the selected month.")] int year,
    [Range(1, 12), Description("Calendar month number, from 1 to 12.")] int month,
    [Range(1, MAX_LOOPBACK_MONTHS), Description("Number of complete months before the selected month to include, from 1 to 12. Defaults to 6.")] int lookbackMonths = 6,
    CancellationToken cancellationToken = default)
  {
    if (year is < 2000 or > MAX_YEAR)
    {
      throw new ArgumentOutOfRangeException(nameof(year), $"year must be between 2000 and {MAX_YEAR}.");
    }

    if (month is < 1 or > 12)
    {
      throw new ArgumentOutOfRangeException(nameof(month), "month must be between 1 and 12.");
    }

    if (lookbackMonths is < 1 or > MAX_LOOPBACK_MONTHS)
    {
      throw new ArgumentOutOfRangeException(nameof(lookbackMonths), $"lookbackMonths must be between 1 and {MAX_LOOPBACK_MONTHS}.");
    }

    var result = await _mediator.Send(
      new GetMonthlyTransactionContextQuery(year, month, lookbackMonths),
      cancellationToken);

    if (!result.IsSuccess || result.Data is null)
    {
      _logger.LogWarning("Could not retrieve monthly transaction context: {Error}", result.ApplicationError?.Message);
      throw new InvalidOperationException("Could not retrieve monthly transaction context for the authenticated user.");
    }

    return result.Data;
  }
}
