using FinanceApp.Backend.Application.Dtos.AIAnalytics;

namespace FinanceApp.Backend.Testing.Unit.AIAnalyticsTests;

public class MonthlyInsightsDtoTests
{
  [Fact]
  public void RequestDto_ExposesMonthAndLookbackValues()
  {
    var dto = new MonthlyInsightsRequestDto { Year = 2026, Month = 12 };

    Assert.Equal(2026, dto.Year);
    Assert.Equal(12, dto.Month);
    Assert.Equal(6, dto.LookbackMonths);
    dto.LookbackMonths = 4;
    Assert.Equal(4, dto.LookbackMonths);
  }

  [Fact]
  public void ResponseDto_ExposesCurrencyAndInsightCards()
  {
    var card = new MonthlyInsightCardDto
    {
      Category = "unusual_transaction",
      Severity = "attention",
      Title = "Unusually large purchase",
      Description = "This purchase is larger than your usual transactions.",
      TransactionIds = [Guid.NewGuid()],
      Amount = 1500m,
      Currency = "USD"
    };
    var dto = new MonthlyInsightsResponseDto
    {
      Year = 2026,
      Month = 12,
      Currency = "USD",
      Insights = [card]
    };

    Assert.Equal(2026, dto.Year);
    Assert.Equal(12, dto.Month);
    Assert.Equal("USD", dto.Currency);
    Assert.Same(card, Assert.Single(dto.Insights));
    Assert.Equal("unusual_transaction", card.Category);
    Assert.Equal("attention", card.Severity);
    Assert.Equal("Unusually large purchase", card.Title);
    Assert.Equal("This purchase is larger than your usual transactions.", card.Description);
    Assert.Single(card.TransactionIds);
    Assert.Equal(1500m, card.Amount);
    Assert.Equal("USD", card.Currency);
  }
}
