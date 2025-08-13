using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Infrastructure.EntityFramework.Mssql.Migrations
{
  /// <inheritdoc />
  public partial class AddResetTokenToUser : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<string>(
          name: "ResetPasswordToken",
          table: "User",
          type: "nvarchar(max)",
          nullable: true);

      migrationBuilder.AddColumn<DateTimeOffset>(
          name: "ResetPasswordTokenExpiration",
          table: "User",
          type: "datetimeoffset",
          nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "ResetPasswordToken",
          table: "User");

      migrationBuilder.DropColumn(
          name: "ResetPasswordTokenExpiration",
          table: "User");
    }
  }
}
