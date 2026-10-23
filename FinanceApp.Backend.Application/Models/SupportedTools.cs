namespace FinanceApp.Backend.Application.Models
{
  public static class SupportedTools
  {
    public const string GET_TOP_TRANSACTION_GROUPS = "GetTopTransactionGroups";

    public static List<string> SupportedToolsList =>
    [
      GET_TOP_TRANSACTION_GROUPS
    ];
  }
}
