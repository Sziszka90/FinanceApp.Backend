using System.Text.Json;
using FinanceApp.Backend.Application.Dtos.LLMProcessorDtos;
using FinanceApp.Backend.Application.Dtos.RabbitMQDtos;

namespace FinanceApp.Backend.Testing.Unit.ClientTests;

public class RabbitMqDtoSerializationTests
{
  [Fact]
  public void PdfImportJob_SerializesPromptAndDocumentPropertiesWithExpectedNames()
  {
    var job = new PdfImportJobDto
    {
      CorrelationId = "correlation-1",
      UserId = "user-1",
      Filename = "statement.pdf",
      PdfBase64 = "JVBERi0x",
      PromptId = "prompt-1",
      PromptName = "My bank",
      Prompt = "Extract transactions"
    };

    var json = JsonSerializer.Serialize(job);
    var roundTrip = JsonSerializer.Deserialize<PdfImportJobDto>(json)!;

    Assert.Contains("\"correlation_id\":\"correlation-1\"", json);
    Assert.Contains("\"pdf_base64\":\"JVBERi0x\"", json);
    Assert.Equal(job.CorrelationId, roundTrip.CorrelationId);
    Assert.Equal(job.UserId, roundTrip.UserId);
    Assert.Equal(job.Filename, roundTrip.Filename);
    Assert.Equal(job.PdfBase64, roundTrip.PdfBase64);
    Assert.Equal(job.PromptId, roundTrip.PromptId);
    Assert.Equal(job.PromptName, roundTrip.PromptName);
    Assert.Equal(job.Prompt, roundTrip.Prompt);
  }

  [Fact]
  public void PdfImportResult_RoundTripsStatementTotalsAndTransactions()
  {
    var result = new PdfImportResultDto
    {
      CorrelationId = "correlation-2",
      UserId = "user-2",
      Success = true,
      Data = new PdfStatementDto
      {
        StatementCurrency = "EUR",
        OpeningBalance = 1000.25m,
        ClosingBalance = 950.25m,
        TotalDebits = 75m,
        TotalCredits = 25m,
        Transactions =
        [
          new PdfTransactionDto
          {
            Description = "Coffee shop",
            Details = "Card payment",
            Date = "2026-10-01",
            SignedAmount = "-4.50",
            Currency = "EUR",
            Reference = "ref-123",
            PageNumber = 2
          }
        ]
      }
    };

    var json = JsonSerializer.Serialize(result);
    var roundTrip = JsonSerializer.Deserialize<PdfImportResultDto>(json)!;
    var transaction = Assert.Single(roundTrip.Data!.Transactions);

    Assert.Contains("\"correlation_id\":\"correlation-2\"", json);
    Assert.Contains("\"statementCurrency\":\"EUR\"", json);
    Assert.Equal(result.CorrelationId, roundTrip.CorrelationId);
    Assert.Equal(result.UserId, roundTrip.UserId);
    Assert.True(roundTrip.Success);
    Assert.Equal("EUR", roundTrip.Data.StatementCurrency);
    Assert.Equal(1000.25m, roundTrip.Data.OpeningBalance);
    Assert.Equal(950.25m, roundTrip.Data.ClosingBalance);
    Assert.Equal(75m, roundTrip.Data.TotalDebits);
    Assert.Equal(25m, roundTrip.Data.TotalCredits);
    Assert.Equal("Coffee shop", transaction.Description);
    Assert.Equal("Card payment", transaction.Details);
    Assert.Equal("2026-10-01", transaction.Date);
    Assert.Equal("-4.50", transaction.SignedAmount);
    Assert.Equal("EUR", transaction.Currency);
    Assert.Equal("ref-123", transaction.Reference);
    Assert.Equal(2, transaction.PageNumber);
  }

  [Fact]
  public void MatchTransactionJob_SerializesQueueContract()
  {
    var job = new MatchTransactionJobDto
    {
      CorrelationId = "correlation-3",
      UserId = "user-3",
      TransactionNames = ["Cafe"],
      TransactionGroupNames = ["Food"]
    };

    var json = JsonSerializer.Serialize(job);
    var roundTrip = JsonSerializer.Deserialize<MatchTransactionJobDto>(json)!;

    Assert.Contains("\"transaction_names\":[\"Cafe\"]", json);
    Assert.Contains("\"transaction_group_names\":[\"Food\"]", json);
    Assert.Equal(job.CorrelationId, roundTrip.CorrelationId);
    Assert.Equal(job.UserId, roundTrip.UserId);
    Assert.Equal(job.TransactionNames, roundTrip.TransactionNames);
    Assert.Equal(job.TransactionGroupNames, roundTrip.TransactionGroupNames);
  }

  [Fact]
  public void PromptRequestAndRabbitMqPayload_RoundTripAllProperties()
  {
    var promptRequest = new PromptRequestDto { Prompt = "Parse statement", CorrelationId = "correlation-4" };
    var payload = new RabbitMqPayload
    {
      CorrelationId = "correlation-4",
      UserId = "user-4",
      Success = false,
      Error = "Could not parse",
      Prompt = "Parse statement",
      Data = new MatchTransactionResponseDto { Transactions = new Dictionary<string, string> { ["Cafe"] = "Food" } }
    };

    var promptRoundTrip = JsonSerializer.Deserialize<PromptRequestDto>(JsonSerializer.Serialize(promptRequest))!;
    var payloadRoundTrip = JsonSerializer.Deserialize<RabbitMqPayload>(JsonSerializer.Serialize(payload))!;

    Assert.Equal(promptRequest.Prompt, promptRoundTrip.Prompt);
    Assert.Equal(promptRequest.CorrelationId, promptRoundTrip.CorrelationId);
    Assert.Equal(payload.CorrelationId, payloadRoundTrip.CorrelationId);
    Assert.Equal(payload.UserId, payloadRoundTrip.UserId);
    Assert.Equal(payload.Success, payloadRoundTrip.Success);
    Assert.Equal(payload.Error, payloadRoundTrip.Error);
    Assert.Equal(payload.Prompt, payloadRoundTrip.Prompt);
    Assert.Equal(payload.Data!.Transactions, payloadRoundTrip.Data!.Transactions);
  }
}
