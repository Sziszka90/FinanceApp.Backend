using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.AuthDtos;
using FinanceApp.Application.Dtos.InvestmentDtos;
using FinanceApp.Application.Dtos.SavingDtos;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceApp.Testing.Base;

public class TestBase : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable, IAsyncDisposable
{
  protected const string TRANSACTIONS = "/api/transactions/";
  protected const string TRANSACTIONS_SUMMARY = "/api/transactions/summary";
  protected const string TRANSACTION_GROUPS = "/api/transactiongroups/";
  protected const string INVESTMENTS = "/api/investments/";
  protected const string SAVINGS = "/api/savings/";
  protected const string USERS = "/api/users/";

  private readonly CustomWebApplicationFactory<Program> _factory;

  protected HttpClient Client { get; }
  protected Guid CreatedUserId { get; set; }

  protected TestBase()
  {
    _factory = new CustomWebApplicationFactory<Program>();
    Client = _factory.CreateClient();
  }

  /// <inheritdoc />
  public async ValueTask DisposeAsync()
  {
    if (_factory.SqliteDatabaseConnection is not null)
    {
      await _factory.SqliteDatabaseConnection.CloseAsync();
    }

    _factory.SqliteDatabaseConnection = null;

    GC.SuppressFinalize(this);
  }

  /// <inheritdoc />
  public void Dispose()
  {
    if (_factory.SqliteDatabaseConnection is not null)
    {
      _factory.SqliteDatabaseConnection.Close();
      _factory.SqliteDatabaseConnection = null;
    }

    GC.SuppressFinalize(this);
  }

  public async Task InitializeAsync()
  {
    CreateDatabaseSchema();
    await GetTokenAsync();
  }

  private void CreateDatabaseSchema()
  {
    using var scope = _factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<FinanceAppDbContext>();
    dbContext.Database.EnsureCreated();
  }

  private async Task GetTokenAsync()
  {
    var user = await CreateUserAsync();
    CreatedUserId = user!.Id;

    var loginContent = CreateContent(new LoginRequestDto
    {
      UserName = user!.UserName,
      Password = "TestPassword90."
    });

    var response = await GetContentAsync<LoginResponseDto>(await Client.PostAsync("/api/auth/login", loginContent));

    Client.DefaultRequestHeaders.Authorization =
      new AuthenticationHeaderValue("Bearer", response!.Token);
  }

  protected async Task<GetInvestmentDto?> CreateInvestmentAsync()
  {
    var investmentContent = CreateContent(new CreateInvestmentDto
    {
      Name = "TestInvestment",
      Value = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = 1000
      },
      Description = "Test Investment"
    });

    var result = await GetContentAsync<GetInvestmentDto>(await Client.PostAsync(INVESTMENTS, investmentContent));
    return result;
  }

  protected async Task<GetSavingDto?> CreateSavingAsync()
  {
    var savingContent = CreateContent(new CreateSavingDto
    {
      Name = "TestSaving",
      Value = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = 1000
      },
      Description = "Test Saving"
    });

    var result = await GetContentAsync<GetSavingDto>(await Client.PostAsync(SAVINGS, savingContent));
    return result;
  }

  protected async Task<GetUserDto?> CreateUserAsync()
  {
    var userContent = CreateContent(new CreateUserDto
    {
      UserName = "test_user90",
      Password = "TestPassword90.",
      BaseCurrency = CurrencyEnum.EUR
    });

    var result = await GetContentAsync<GetUserDto>(await Client.PostAsync(USERS, userContent));
    return result;
  }

  protected async Task<GetTransactionDto?> CreateTransactionAsync()
  {
    var transactionGroupContent = CreateContent(new CreateTransactionGroupDto
    {
      Name = "TransactionGroup",
      Description = "Transaction group",
      GroupIcon = "icon",
      Limit = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = 100
      }
    });

    var transactionGroup = await GetContentAsync<GetTransactionGroupDto>(await Client.PostAsync(TRANSACTION_GROUPS, transactionGroupContent));

    var transactionContent = CreateContent(new CreateTransactionDto
    {
      Name = "TestTransaction",
      Description = "Test Transaction",
      Value = new Money
      {
        Amount = 100,
        Currency = CurrencyEnum.HUF
      },
      TransactionType = TransactionTypeEnum.Expense,
      TransactionDate = new DateTimeOffset(),
      TransactionGroupId = transactionGroup!.Id
    });

    var result = await GetContentAsync<GetTransactionDto>(await Client.PostAsync(TRANSACTIONS, transactionContent));
    return result;
  }

  protected async Task<GetTransactionGroupDto?> CreateTransactionGroupAsync()
  {
    var transactionGroupContent = CreateContent(new CreateTransactionGroupDto
    {
      Name = "TransactionGroup",
      Description = "Transaction group",
      Limit = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = 100
      },
      GroupIcon = "icon"
    });

    var result = await GetContentAsync<GetTransactionGroupDto>(await Client.PostAsync(TRANSACTION_GROUPS, transactionGroupContent));

    return result;
  }

  protected async Task<List<GetTransactionDto>> CreateMultipleTransactionAsync()
  {
    var transactionGroupContent = CreateContent(new CreateTransactionGroupDto
    {
      Name = "TransactionGroup",
      Description = "Transaction group",
      Limit = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = 100
      }
    });

    var transactionGroup = await GetContentAsync<GetTransactionGroupDto>(await Client.PostAsync(TRANSACTION_GROUPS, transactionGroupContent));

    var transactionList = new List<GetTransactionDto>();

    var transactionContentHuf = CreateContent(new CreateTransactionDto
    {
      Name = "TestTransaction1",
      Description = "Test Transaction",
      Value = new Money
      {
        Amount = 1000000,
        Currency = CurrencyEnum.HUF
      },
      TransactionDate = new DateTimeOffset(),
      TransactionGroupId = transactionGroup!.Id
    });

    transactionList.Add((await GetContentAsync<GetTransactionDto>(await Client.PostAsync(TRANSACTIONS, transactionContentHuf)))!);

    var transactionContentGbp = CreateContent(new CreateTransactionDto
    {
      Name = "TestTransaction2",
      Description = "Test Transaction",
      Value = new Money
      {
        Amount = 10,
        Currency = CurrencyEnum.GBP
      },
      TransactionDate = new DateTimeOffset(),
      TransactionGroupId = transactionGroup!.Id
    });

    transactionList.Add((await GetContentAsync<GetTransactionDto>(await Client.PostAsync(TRANSACTIONS, transactionContentGbp)))!);

    return transactionList;
  }

  protected async Task<T?> GetContentAsync<T>(HttpResponseMessage message)
  {
    var options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };

    options.Converters.Add(new JsonStringEnumConverter());

    var objectAsString = await message.Content.ReadAsStringAsync();
    return string.IsNullOrEmpty(objectAsString) ? default : JsonSerializer.Deserialize<T>(objectAsString, options);
    ;
  }

  protected StringContent CreateContent<T>(T objectToConvert)
  {
    var objectAsString = JsonSerializer.Serialize(objectToConvert);
    var result = new StringContent(objectAsString, Encoding.UTF8, "application/json");
    return result;
  }
}
