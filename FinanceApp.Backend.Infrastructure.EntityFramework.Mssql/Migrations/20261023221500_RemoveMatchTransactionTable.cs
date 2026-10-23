using FinanceApp.Backend.Infrastructure.EntityFramework.Mssql.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Mssql.Migrations;

[DbContext(typeof(FinanceAppMssqlDbContext))]
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
        Transaction = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
        TransactionGroup = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
        CorrelationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
        Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        Modified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_MatchTransaction", row => new { row.Transaction, row.TransactionGroup });
      });
  }
}
