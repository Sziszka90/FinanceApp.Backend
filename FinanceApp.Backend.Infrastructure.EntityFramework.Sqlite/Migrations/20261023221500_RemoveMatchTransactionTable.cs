using FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite.Migrations;

[DbContext(typeof(FinanceAppSqliteDbContext))]
[Migration("20261023221500_RemoveMatchTransactionTable")]
public partial class RemoveMatchTransactionTable : Migration
{
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(name: "MatchTransaction");
  }

  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
      name: "MatchTransaction",
      columns: table => new
      {
        Transaction = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
        TransactionGroup = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
        CorrelationId = table.Column<string>(type: "TEXT", nullable: false),
        Created = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
        Id = table.Column<Guid>(type: "TEXT", nullable: false),
        Modified = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_MatchTransaction", row => new { row.Transaction, row.TransactionGroup });
      });
  }
}
