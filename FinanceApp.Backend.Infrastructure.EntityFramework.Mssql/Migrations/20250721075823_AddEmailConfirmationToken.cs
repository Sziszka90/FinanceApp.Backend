using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Infrastructure.EntityFramework.Mssql.Migrations
{
  /// <inheritdoc />
  public partial class AddEmailConfirmationToken : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<string>(
          name: "EmailConfirmationToken",
          table: "User",
          type: "nvarchar(max)",
          nullable: true);

      migrationBuilder.AddColumn<DateTimeOffset>(
          name: "EmailConfirmationTokenExpiration",
          table: "User",
          type: "datetimeoffset",
          nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "EmailConfirmationToken",
          table: "User");

      migrationBuilder.DropColumn(
          name: "EmailConfirmationTokenExpiration",
          table: "User");
    }
  }
}
