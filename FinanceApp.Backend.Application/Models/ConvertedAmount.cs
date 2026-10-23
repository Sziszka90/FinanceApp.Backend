namespace FinanceApp.Backend.Application.Models;

public record ConvertedAmount(decimal Amount, decimal? AppliedRate, DateTimeOffset? RateValidFrom);
