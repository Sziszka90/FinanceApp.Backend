using FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite.Migrations;

[DbContext(typeof(FinanceAppSqliteDbContext))]
[Migration("20261023182000_SaveAppliedExchangeRate")]
public partial class SaveAppliedExchangeRate : Migration
{
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AddColumn<decimal>(
      name: "AppliedExchangeRate", table: "Transaction", type: "decimal(18,6)", nullable: true);
    migrationBuilder.AddColumn<DateTimeOffset>(
      name: "AppliedRateValidFrom", table: "Transaction", type: "TEXT", nullable: true);
  }

  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropColumn(name: "AppliedExchangeRate", table: "Transaction");
    migrationBuilder.DropColumn(name: "AppliedRateValidFrom", table: "Transaction");
  }
}
