using FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite.ContextFactory;

public class FinanceAppSqliteDbContextFactory : IDesignTimeDbContextFactory<FinanceAppSqliteDbContext>
{
  public FinanceAppSqliteDbContext CreateDbContext(string[] args)
  {
    var options = new DbContextOptionsBuilder<FinanceAppSqliteDbContext>()
      .UseSqlite("Data Source=financeapp-design.db")
      .Options;

    return new FinanceAppSqliteDbContext(options);
  }
}
