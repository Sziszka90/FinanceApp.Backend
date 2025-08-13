using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FinanceApp.Backend.Application.Dtos.AuthDtos;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceApp.Backend.Testing.Api.Base;

public class TestBase : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable, IAsyncDisposable
{
  protected const string TRANSACTIONS = "/api/v1/transactions/";
  protected const string TRANSACTIONS_SUMMARY = "/api/v1/transactions/summary";
  protected const string TRANSACTION_GROUPS = "/api/v1/transactiongroups/";
  protected const string INVESTMENTS = "/api/v1/investments/";
  protected const string SAVINGS = "/api/v1/savings/";
  protected const string USERS = "/api/v1/users/";
  protected const string AUTH = "/api/v1/auth/";

  private readonly CustomWebApplicationFactory<Program> _factory;

  protected HttpClient Client { get; }
  protected Guid CreatedUserId { get; set; }
  protected string Token { get; set; } = string.Empty;

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
    await GenerateExchangeRatesAsync();
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

    await ConfirmUserEmailAsync();

    var loginContent = CreateContent(new LoginRequestDto
    {
      Email = user!.Email,
      Password = "TestPassword90."
    });

    var response = await GetContentAsync<LoginResponseDto>(await Client.PostAsync(AUTH + "login", loginContent));

    Token = response!.Token;

    Client.DefaultRequestHeaders.Authorization =
      new AuthenticationHeaderValue("Bearer", response!.Token);
  }

  private async Task GenerateExchangeRatesAsync()
  {
    using var scope = _factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<FinanceAppDbContext>();
    var exchangeRate = new ExchangeRate(
      CurrencyEnum.HUF.ToString(),
      CurrencyEnum.EUR.ToString(),
      404.8m
    );
    dbContext.ExchangeRate.Add(exchangeRate);
    await dbContext.SaveChangesAsync();
  }

  protected async Task ConfirmUserEmailAsync()
  {
    using var scope = _factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<FinanceAppDbContext>();
    var user = await dbContext.User.FindAsync(CreatedUserId);
    if (user != null)
    {
      user.IsEmailConfirmed = true;
      await dbContext.SaveChangesAsync();
    }
  }

  protected async Task<GetUserDto?> CreateUserAsync()
  {
    var userContent = CreateContent(new CreateUserDto
    {
      UserName = "test_user90",
      Password = "TestPassword90.",
      Email = "test_user90@example.com",
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
    });

    var transactionGroup = await GetContentAsync<GetTransactionGroupDto>(await Client.PostAsync(TRANSACTION_GROUPS, transactionGroupContent));

    var transaction = new CreateTransactionDto
    {
      Name = "TestTransaction",
      Description = "Test Transaction",
      Value = new Money
      {
        Amount = 100,
        Currency = CurrencyEnum.HUF
      },
      TransactionType = TransactionTypeEnum.Expense,
      TransactionDate = DateTimeOffset.Now,
      TransactionGroupId = transactionGroup!.Id.ToString()
    };
    var transactionContent = CreateContent(transaction);

    var result = await GetContentAsync<GetTransactionDto>(await Client.PostAsync(TRANSACTIONS, transactionContent));
    return result;
  }

  protected async Task<List<GetTransactionDto?>> CreateMultipleTransactionAsync()
  {
    var createdTransactionGroups = new List<GetTransactionGroupDto>();
    var createdTransactions = new List<GetTransactionDto?>();

    List<CreateTransactionGroupDto> transactionGroupsToCreate = new List<CreateTransactionGroupDto>
    {
      new() {
        Name = "TransactionGroup_1",
        Description = "Transaction group",
        GroupIcon = "icon"
      },
      new() {
        Name = "TransactionGroup_2",
        Description = "Transaction group",
        GroupIcon = "icon"
      },
      new()
      {
        Name = "TransactionGroup_3",
        Description = "Transaction group",
        GroupIcon = "icon"
      }
    };

    foreach (var transactionGroupToCreate in transactionGroupsToCreate)
    {
      var transactionGroupContent = CreateContent(transactionGroupToCreate);
      var transactionGroup = await GetContentAsync<GetTransactionGroupDto>(await Client.PostAsync(TRANSACTION_GROUPS, transactionGroupContent));
      createdTransactionGroups.Add(transactionGroup!);
    }

    List<CreateTransactionDto> transactionsToCreate = new List<CreateTransactionDto>()
    {
      new CreateTransactionDto
      {
        Name = "TestTransaction_1",
        Description = "Test Transaction",
        Value = new Money
        {
          Amount = 100,
          Currency = CurrencyEnum.HUF
        },
        TransactionType = TransactionTypeEnum.Expense,
        TransactionDate = new DateTimeOffset(),
        TransactionGroupId = createdTransactionGroups[0]!.Id.ToString()
      },
      new CreateTransactionDto
      {
        Name = "TestTransaction_2",
        Description = "Test Transaction",
        Value = new Money
        {
          Amount = 100,
          Currency = CurrencyEnum.HUF
        },
        TransactionType = TransactionTypeEnum.Expense,
        TransactionDate = new DateTimeOffset(),
        TransactionGroupId = createdTransactionGroups[1]!.Id.ToString()
      },
      new CreateTransactionDto
      {
        Name = "TestTransaction_3",
        Description = "Test Transaction",
        Value = new Money
        {
          Amount = 100,
          Currency = CurrencyEnum.HUF
        },
        TransactionType = TransactionTypeEnum.Expense,
        TransactionDate = new DateTimeOffset(),
        TransactionGroupId = createdTransactionGroups[2]!.Id.ToString()
      }
    };

    foreach (var transactionToCreate in transactionsToCreate)
    {
      var transactionContent = CreateContent(transactionToCreate);
      var transaction = await GetContentAsync<GetTransactionDto>(await Client.PostAsync(TRANSACTIONS, transactionContent));
      createdTransactions.Add(transaction);
    }

    return createdTransactions;
  }

  protected async Task<GetTransactionGroupDto?> CreateTransactionGroupAsync()
  {
    var transactionGroupContent = CreateContent(new CreateTransactionGroupDto
    {
      Name = "TransactionGroup",
      Description = "Transaction group",
      GroupIcon = "icon"
    });

    var result = await GetContentAsync<GetTransactionGroupDto>(await Client.PostAsync(TRANSACTION_GROUPS, transactionGroupContent));

    return result;
  }

  protected async Task<GetTransactionGroupDto?> CreateTransactionGroupAsync(CreateTransactionGroupDto createTransactionGroupDto)
  {
    var transactionGroupContent = CreateContent(createTransactionGroupDto);

    var result = await GetContentAsync<GetTransactionGroupDto>(await Client.PostAsync(TRANSACTION_GROUPS, transactionGroupContent));

    return result;
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
