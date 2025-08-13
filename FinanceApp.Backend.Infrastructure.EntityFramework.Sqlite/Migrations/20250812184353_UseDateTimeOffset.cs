using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Infrastructure.EntityFramework.Sqlite.Migrations
{
  /// <inheritdoc />
  public partial class UseDateTimeOffset : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "TransactionDate",
          table: "Transaction",
          type: "datetimeoffset",
          nullable: false,
          oldClrType: typeof(DateTimeOffset),
          oldType: "TEXT");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<DateTimeOffset>(
          name: "TransactionDate",
          table: "Transaction",
          type: "TEXT",
          nullable: false,
          oldClrType: typeof(DateTimeOffset),
          oldType: "datetimeoffset");
    }
  }
}
