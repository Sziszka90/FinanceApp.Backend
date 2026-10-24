using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Mssql.Migrations
{
  /// <inheritdoc />
  public partial class SaveAppliedExchangeRate : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<decimal>(
          name: "AppliedExchangeRate",
          table: "Transaction",
          type: "decimal(18,6)",
          nullable: true);

      migrationBuilder.AddColumn<DateTimeOffset>(
          name: "AppliedRateValidFrom",
          table: "Transaction",
          type: "datetimeoffset",
          nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "AppliedExchangeRate",
          table: "Transaction");

      migrationBuilder.DropColumn(
          name: "AppliedRateValidFrom",
          table: "Transaction");
    }
  }
}
