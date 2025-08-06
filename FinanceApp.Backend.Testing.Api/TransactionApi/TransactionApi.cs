using System.Net;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using FinanceApp.Backend.Testing.Api.Base;

namespace FinanceApp.Backend.Testing.Api.TransactionApi;

public class TransactionApi : TestBase
{
  [Fact]
  public async Task CreateTransaction_WhenTransactionGroupDoesNotExists_ReturnsNotExists()
  {
    // arrange
    await InitializeAsync();
    var transactionContent = CreateContent(new CreateTransactionDto
    {
      Name = "TestTransaction",
      Description = "Test Transaction",
      Value = new Money
      {
        Amount = 100,
        Currency = CurrencyEnum.HUF
      },
      TransactionDate = new DateTimeOffset(),
      TransactionGroupId = new Guid().ToString()
    });

    // act
    var response = await Client.PostAsync(TRANSACTIONS, transactionContent);
    var content = await response.Content.ReadAsStringAsync();

    // assert
    Assert.Contains(ApplicationError.TRANSACTION_GROUP_NOT_EXISTS_CODE, content);
  }

  [Fact]
  public async Task DeleteTransaction_DoesNotDeleteTransactionGroup()
  {
    // arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();
    await Client.DeleteAsync(TRANSACTIONS + transaction!.Id);

    // act
    var response = await GetContentAsync<GetTransactionGroupDto>(await Client.GetAsync(TRANSACTION_GROUPS + transaction!.TransactionGroup!.Id));

    // assert
    Assert.Equal(response!.Id, transaction.TransactionGroup!.Id);
  }

  [Fact]
  public async Task DeleteTransaction_ReturnsNothing()
  {
    // arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();

    // act
    var response = await Client.DeleteAsync(TRANSACTIONS + transaction!.Id);

    // assert
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }

  [Fact]
  public async Task DeleteTransactionAndTransactionGroup_ReturnsNoContent()
  {
    // arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();


    // act

    var responseTransaction = await Client.DeleteAsync(TRANSACTIONS + transaction!.Id);
    var responseTransactionGroup = await Client.DeleteAsync(TRANSACTION_GROUPS + transaction!.TransactionGroup!.Id);

    // assert
    Assert.Equal(HttpStatusCode.NoContent, responseTransaction.StatusCode);
    Assert.Equal(HttpStatusCode.NoContent, responseTransactionGroup.StatusCode);
  }

  [Fact]
  public async Task DeleteNotExistingTransaction_ReturnsNotFound()
  {
    // arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();
    transaction!.Id = Guid.NewGuid();

    // act
    var response = await Client.DeleteAsync(TRANSACTIONS + transaction!.Id);

    // assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }


  [Fact]
  public async Task GetAllTransaction_ReturnsValidTransaction()
  {
    // arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();

    // act
    var response = await GetContentAsync<List<GetTransactionDto>>(await Client.GetAsync(TRANSACTIONS));

    // assert
    Assert.Equal(transaction!.Id, response![0].Id);
  }

  [Fact]
  public async Task GetAllTransaction_ByFilterTransactionGroup_ReturnsValidTransaction()
  {
    // arrange
    await InitializeAsync();
    var transactions = await CreateMultipleTransactionAsync();

    // act
    var response = await GetContentAsync<List<GetTransactionDto>>(await Client.GetAsync(TRANSACTIONS + $"?TransactionGroupName={transactions[0]!.TransactionGroup!.Name}"));

    // assert
    Assert.Equal(transactions[0]!.Id, response![0].Id);
  }

  [Fact]
  public async Task GetAllTransaction_ByFilterTransactionName_ReturnsValidTransaction()
  {
    // arrange
    await InitializeAsync();
    var transactions = await CreateMultipleTransactionAsync();

    // act
    var response = await GetContentAsync<List<GetTransactionDto>>(await Client.GetAsync(TRANSACTIONS + $"?TransactionName={transactions[0]!.Name}"));

    // assert
    Assert.Equal(transactions[0]!.Id, response![0].Id);
  }

  [Fact]
  public async Task GetAllTransaction_ByFilterTransactionDateAscending_ReturnsValidTransaction()
  {
    // arrange
    await InitializeAsync();
    var transactions = await CreateMultipleTransactionAsync();

    // act
    var response = await GetContentAsync<List<GetTransactionDto>>(await Client.GetAsync(TRANSACTIONS + $"?OrderBy=TransactionDate&Ascending=true"));

    // assert
    Assert.Equal(transactions[0]!.Id, response![0].Id);
  }

  [Fact]
  public async Task GetAllTransactionSummary_ReturnsAllTransactionSummary()
  {
    // arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();

    // act
    var response = await Client.GetAsync(TRANSACTIONS_SUMMARY);
    var sum = await GetContentAsync<Money>(response);

    // assert
    Assert.True(sum!.Amount > transaction?.Value.Amount);
  }

  [Fact]
  public async Task GetTransactionById_ReturnsValidTransaction()
  {
    // arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();

    // act
    var response = await GetContentAsync<GetTransactionDto>(await Client.GetAsync(TRANSACTIONS + transaction!.Id));

    // assert
    Assert.Equal(transaction!.Id, response!.Id);
  }

  [Fact]
  public async Task UpdateTransaction_ReturnsUpdatedTransaction()
  {
    // arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();
    var updatedTransaction = new UpdateTransactionDto
    {
      Id = transaction!.Id,
      Name = "Updated Name",
      Description = "Updated Description",
      Value = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = 200
      },
      TransactionDate = transaction.TransactionDate,
      TransactionGroupId = transaction.TransactionGroup!.Id
    };

    // act
    await GetContentAsync<GetTransactionDto>(await Client.PutAsync(TRANSACTIONS + transaction!.Id, CreateContent(updatedTransaction)));
    var response = await GetContentAsync<GetTransactionDto>(await Client.GetAsync(TRANSACTIONS + transaction!.Id));

    // assert
    Assert.Equal(transaction!.Id, response!.Id);
    Assert.Equal(updatedTransaction.Name, response.Name);
    Assert.Equal(updatedTransaction.Value.Amount, response.Value.Amount);
  }

  [Fact]
  public async Task UpdateTransactionNegativeValue_ReturnsValidationError()
  {
    // arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();
    var updatedTransaction = new UpdateTransactionDto
    {
      Id = transaction!.Id,
      Name = "Updated Name",
      Description = "Updated Description",
      Value = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = -200
      },
      TransactionDate = transaction.TransactionDate,
      TransactionGroupId = transaction.TransactionGroup!.Id
    };

    // act
    var response = await Client.PutAsync(TRANSACTIONS + transaction!.Id, CreateContent(updatedTransaction));
    var responseContentAsString = await response.Content.ReadAsStringAsync();

    // assert
    Assert.Contains(ApplicationError.VALIDATION_MESSAGE, responseContentAsString);
  }

  [Fact]
  public async Task CreateTransaction_WithValidData_ReturnsCreatedTransaction()
  {
    // arrange
    await InitializeAsync();
    var transactionGroup = await CreateTransactionGroupAsync();
    var createTransactionDto = new CreateTransactionDto
    {
      Name = "New Transaction",
      Description = "Test transaction",
      Value = new Money
      {
        Amount = 150.75m,
        Currency = CurrencyEnum.EUR
      },
      TransactionType = TransactionTypeEnum.Income,
      TransactionDate = DateTimeOffset.Now,
      TransactionGroupId = transactionGroup!.Id.ToString()
    };

    // act
    var response = await Client.PostAsync(TRANSACTIONS, CreateContent(createTransactionDto));
    var createdTransaction = await GetContentAsync<GetTransactionDto>(response);

    // assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(createdTransaction);
    Assert.Equal(createTransactionDto.Name, createdTransaction.Name);
    Assert.Equal(createTransactionDto.Value.Amount, createdTransaction.Value.Amount);
    Assert.Equal(createTransactionDto.Value.Currency, createdTransaction.Value.Currency);
  }

  [Fact]
  public async Task CreateTransaction_WithInvalidTransactionGroup_ReturnsBadRequest()
  {
    // arrange
    await InitializeAsync();
    var createTransactionDto = new CreateTransactionDto
    {
      Name = "Invalid Transaction",
      Description = "Transaction with invalid group",
      Value = new Money
      {
        Amount = 100,
        Currency = CurrencyEnum.USD
      },
      TransactionType = TransactionTypeEnum.Expense,
      TransactionDate = DateTimeOffset.Now,
      TransactionGroupId = Guid.NewGuid().ToString()
    };

    // act
    var response = await Client.PostAsync(TRANSACTIONS, CreateContent(createTransactionDto));
    var responseContent = await response.Content.ReadAsStringAsync();

    // assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    Assert.Contains(ApplicationError.TRANSACTION_GROUP_NOT_EXISTS_CODE, responseContent);
  }

  [Fact]
  public async Task CreateTransaction_WithNegativeAmount_ReturnsValidationError()
  {
    // arrange
    await InitializeAsync();
    var transactionGroup = await CreateTransactionGroupAsync();
    var createTransactionDto = new CreateTransactionDto
    {
      Name = "Invalid Transaction",
      Description = "Transaction with negative amount",
      Value = new Money
      {
        Amount = -50,
        Currency = CurrencyEnum.USD
      },
      TransactionType = TransactionTypeEnum.Expense,
      TransactionDate = DateTimeOffset.Now,
      TransactionGroupId = transactionGroup!.Id.ToString()
    };

    // act
    var response = await Client.PostAsync(TRANSACTIONS, CreateContent(createTransactionDto));
    var responseContent = await response.Content.ReadAsStringAsync();

    // assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    Assert.Contains(ApplicationError.VALIDATION_MESSAGE, responseContent);
  }

  [Fact]
  public async Task GetTransactionById_WithInvalidId_ReturnsNotFound()
  {
    // arrange
    await InitializeAsync();
    var invalidId = Guid.NewGuid();

    // act
    var response = await Client.GetAsync(TRANSACTIONS + invalidId);

    // assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task UpdateTransaction_WithInvalidId_ReturnsNotFound()
  {
    // arrange
    await InitializeAsync();
    var transactionGroup = await CreateTransactionGroupAsync();
    var transaction = await CreateTransactionAsync();
    var invalidId = Guid.NewGuid();
    var updateDto = new UpdateTransactionDto
    {
      Id = invalidId,
      Name = "Updated Name",
      Description = "Updated Description",
      Value = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = 100
      },
      TransactionDate = DateTimeOffset.Now,
      TransactionGroupId = transactionGroup!.Id
    };

    // act
    var response = await Client.PutAsync(TRANSACTIONS + invalidId, CreateContent(updateDto));

    // assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task UpdateTransaction_WithInvalidTransactionGroup_ReturnsBadRequest()
  {
    // arrange
    await InitializeAsync();
    var transaction = await CreateTransactionAsync();
    var updateDto = new UpdateTransactionDto
    {
      Id = transaction!.Id,
      Name = "Updated Name",
      Description = "Updated Description",
      Value = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = 100
      },
      TransactionDate = transaction.TransactionDate,
      TransactionGroupId = Guid.NewGuid() // Invalid transaction group
    };

    // act
    var response = await Client.PutAsync(TRANSACTIONS + transaction.Id, CreateContent(updateDto));
    var responseContent = await response.Content.ReadAsStringAsync();

    // assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    Assert.Contains(ApplicationError.TRANSACTION_GROUP_NOT_EXISTS_CODE, responseContent);
  }

  [Fact]
  public async Task GetAllTransactions_ByDateRange_ReturnsFilteredResults()
  {
    // arrange
    await InitializeAsync();
    await CreateMultipleTransactionAsync();
    var startDate = DateTimeOffset.Now.AddDays(-1).ToString("yyyy-MM-dd");
    var endDate = DateTimeOffset.Now.AddDays(1).ToString("yyyy-MM-dd");

    // act
    var response = await GetContentAsync<List<GetTransactionDto>>(
      await Client.GetAsync(TRANSACTIONS + $"?StartDate={startDate}&EndDate={endDate}"));

    // assert
    Assert.NotNull(response);
    Assert.True(response.Count > 0);
  }

  [Fact]
  public async Task GetAllTransactions_ByTransactionType_ReturnsFilteredResults()
  {
    // arrange
    await InitializeAsync();
    await CreateMultipleTransactionAsync();

    // act
    var response = await GetContentAsync<List<GetTransactionDto>>(
      await Client.GetAsync(TRANSACTIONS + "?TransactionType=Expense"));

    // assert
    Assert.NotNull(response);
    Assert.All(response, t => Assert.Equal(TransactionTypeEnum.Expense, t.TransactionType));
  }

  [Fact]
  public async Task UploadCsv_WithValidFile_ReturnsSuccess()
  {
    // arrange
    await InitializeAsync();
    var csvContent = "Username,Account Number,Booking Date,Amount,Currency,Partner Name,Partner IBAN,Partner Account Number,Partner Bank Code,Booking Information,Transaction ID,Transaction Date and Time\n" +
                    "testuser,Account1,2024-01-01,100.50,USD,Coffee Shop,,,,,TXN001,2024-01-01 10:00:00";

    var formData = new MultipartFormDataContent();
    var fileContent = new StringContent(csvContent);
    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
    formData.Add(fileContent, "file", "test.csv");
    formData.Add(new StringContent(Guid.NewGuid().ToString()), "correlationId");

    // act
    var response = await Client.PostAsync(TRANSACTIONS + "import", formData);

    // assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task UploadCsv_WithInvalidFileType_ReturnsBadRequest()
  {
    // arrange
    await InitializeAsync();
    var textContent = "This is not a CSV file";

    var formData = new MultipartFormDataContent();
    var fileContent = new StringContent(textContent);
    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
    formData.Add(fileContent, "file", "test.txt");
    formData.Add(new StringContent(Guid.NewGuid().ToString()), "correlationId");

    // act
    var response = await Client.PostAsync(TRANSACTIONS + "import", formData);

    // assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task UploadCsv_WithoutFile_ReturnsBadRequest()
  {
    // arrange
    await InitializeAsync();
    var formData = new MultipartFormDataContent();
    formData.Add(new StringContent(Guid.NewGuid().ToString()), "correlationId");

    // act
    var response = await Client.PostAsync(TRANSACTIONS + "import", formData);

    // assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task UploadCsv_WithoutCorrelationId_ReturnsBadRequest()
  {
    // arrange
    await InitializeAsync();
    var csvContent = "Username,Account Number,Booking Date,Amount,Currency\ntestuser,Account1,2024-01-01,100,USD";

    var formData = new MultipartFormDataContent();
    var fileContent = new StringContent(csvContent);
    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
    formData.Add(fileContent, "file", "test.csv");

    // act
    var response = await Client.PostAsync(TRANSACTIONS + "import", formData);

    // assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task GetAllTransactionSummary_WhenNoTransactions_ReturnsZeroSum()
  {
    // arrange
    await InitializeAsync();

    // act
    var response = await Client.GetAsync(TRANSACTIONS_SUMMARY);
    var sum = await GetContentAsync<Money>(response);

    // assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(sum);
    Assert.Equal(0, sum.Amount);
  }

  [Fact]
  public async Task GetAllTransactions_WhenEmpty_ReturnsEmptyList()
  {
    // arrange
    await InitializeAsync();

    // act
    var response = await GetContentAsync<List<GetTransactionDto>>(await Client.GetAsync(TRANSACTIONS));

    // assert
    Assert.NotNull(response);
    Assert.Empty(response);
  }
}
