namespace FinanceApp.Backend.Application.Models
{
  public static class SupportedTools
  {
    public const string GET_TOP_TRANSACTION_GROUPS = "get_top_transaction_groups";

    public static List<string> SupportedActions =>
    [
      GET_TOP_TRANSACTION_GROUPS
    ];
  }
}
