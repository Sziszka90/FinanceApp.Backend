using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.AuthDtos;
using FinanceApp.Application.Dtos.ExpenseTransactionDtos;
using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Application.Dtos.IncomeTransactionDtos;
using FinanceApp.Application.Dtos.IncomeTransactionGroupDtos;
using FinanceApp.Application.Dtos.InvestmentDtos;
using FinanceApp.Application.Dtos.SavingDtos;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceApp.Testing.Base;

public class TestBase : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable, IAsyncDisposable
{
  #region Constants

  protected const string INCOME_TRANSACTIONS = "/api/incometransactions/";
  protected const string INCOME_TRANSACTIONS_SUMMARY = "/api/incometransactions/summary";
  protected const string INCOME_TRANSACTION_GROUPS = "/api/incometransactiongroups/";

  protected const string EXPENSE_TRANSACTIONS = "/api/expensetransactions/";
  protected const string EXPENSE_TRANSACTION_GROUPS = "/api/expensetransactiongroups/";

  protected const string INVESTMENTS = "/api/investments/";

  protected const string SAVINGS = "/api/savings/";
  protected const string USERS = "/api/users/";

  #endregion

  #region Members

  private readonly CustomWebApplicationFactory<Program> _factory;

  #endregion

  #region Properties

  protected HttpClient Client { get; }
  protected Guid CreatedUserId { get; set; }

  #endregion

  #region Constructors

  protected TestBase()
  {
    _factory = new CustomWebApplicationFactory<Program>();
    Client = _factory.CreateClient();
  }

  #endregion

  #region Methods

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
    var dbContext = _factory.Services.GetRequiredService<FinanceAppDbContext>();
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

  #endregion

  protected async Task<GetInvestmentDto?> CreateInvestmentAsync()
  {
    var investmentContent = CreateContent(new CreateInvestmentDto
    {
      Name = "TestInvestment",
      Amount = new Money
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
      Amount = new Money
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

  protected async Task<GetExpenseTransactionDto?> CreateExpenseAsync()
  {
    var expenseGroupContent = CreateContent(new CreateExpenseTransactionGroupDto
    {
      Name = "ExpenseGroup",
      Description = "Expense group",
      Icon = "icon",
      Limit = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = 100
      }
    });

    var expenseGroup = await GetContentAsync<GetExpenseTransactionGroupDto>(await Client.PostAsync(EXPENSE_TRANSACTION_GROUPS, expenseGroupContent));

    var expenseContent = CreateContent(new CreateExpenseTransactionDto
    {
      Name = "TestExpense",
      Description = "Test Expense",
      Value = new Money
      {
        Amount = 100,
        Currency = CurrencyEnum.HUF
      },
      DueDate = new DateTimeOffset(),
      TransactionGroupId = expenseGroup!.Id
    });

    var result = await GetContentAsync<GetExpenseTransactionDto>(await Client.PostAsync(EXPENSE_TRANSACTIONS, expenseContent));
    return result;
  }

  protected async Task<GetIncomeTransactionGroupDto?> CreateIncomeTransactionGroupAsync()
  {
    var incomeTransactionGroupContent = CreateContent(new CreateIncomeTransactionGroupDto
    {
      Name = "ExpenseGroup",
      Description = "Expense group",
      Icon = "icon"
    });

    var result = await GetContentAsync<GetIncomeTransactionGroupDto>(await Client.PostAsync(INCOME_TRANSACTION_GROUPS, incomeTransactionGroupContent));

    return result;
  }

  protected async Task<GetExpenseTransactionGroupDto?> CreateExpenseTransactionGroupAsync()
  {
    var expenseTransactionGroupContent = CreateContent(new CreateExpenseTransactionGroupDto
    {
      Name = "ExpenseGroup",
      Description = "Expense group",
      Icon = "icon",
      Limit = new Money
      {
        Currency = CurrencyEnum.USD,
        Amount = 100
      }
    });

    var result = await GetContentAsync<GetExpenseTransactionGroupDto>(await Client.PostAsync(EXPENSE_TRANSACTION_GROUPS, expenseTransactionGroupContent));

    return result;
  }

  protected async Task<GetIncomeTransactionDto?> CreateIncomeAsync()
  {
    var incomeGroupContent = CreateContent(new CreateIncomeTransactionGroupDto
    {
      Name = "IncomeGroup",
      Description = "Income group",
      Icon = "icon"
    });

    var incomeGroup = await GetContentAsync<GetIncomeTransactionGroupDto>(await Client.PostAsync(INCOME_TRANSACTION_GROUPS, incomeGroupContent));

    var incomeContent = CreateContent(new CreateIncomeTransactionDto
    {
      Name = "TestIncome",
      Description = "Test Income",
      Value = new Money
      {
        Amount = 100,
        Currency = CurrencyEnum.HUF
      },
      DueDate = new DateTimeOffset(),
      TransactionGroupId = incomeGroup!.Id
    });

    var result = await GetContentAsync<GetIncomeTransactionDto>(await Client.PostAsync(INCOME_TRANSACTIONS, incomeContent));
    return result;
  }

  protected async Task<List<GetIncomeTransactionDto>> CreateMultipleIncomeAsync()
  {
    var incomeGroupContent = CreateContent(new CreateIncomeTransactionGroupDto
    {
      Name = "IncomeGroup",
      Description = "Income group",
      Icon = "icon"
    });

    var incomeGroup = await GetContentAsync<GetIncomeTransactionGroupDto>(await Client.PostAsync(INCOME_TRANSACTION_GROUPS, incomeGroupContent));

    var inComeList = new List<GetIncomeTransactionDto>();

    var incomeContentHuf = CreateContent(new CreateIncomeTransactionDto
    {
      Name = "TestIncome1",
      Description = "Test Income",
      Value = new Money
      {
        Amount = 1000000,
        Currency = CurrencyEnum.HUF
      },
      DueDate = new DateTimeOffset(),
      TransactionGroupId = incomeGroup!.Id
    });

    inComeList.Add((await GetContentAsync<GetIncomeTransactionDto>(await Client.PostAsync(INCOME_TRANSACTIONS, incomeContentHuf)))!);

    var incomeContentGbp = CreateContent(new CreateIncomeTransactionDto
    {
      Name = "TestIncome2",
      Description = "Test Income",
      Value = new Money
      {
        Amount = 10,
        Currency = CurrencyEnum.GBP
      },
      DueDate = new DateTimeOffset(),
      TransactionGroupId = incomeGroup!.Id
    });

    inComeList.Add((await GetContentAsync<GetIncomeTransactionDto>(await Client.PostAsync(INCOME_TRANSACTIONS, incomeContentGbp)))!);

    return inComeList;
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