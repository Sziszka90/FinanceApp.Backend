using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Infrastructure.EntityFramework.Sqlite.Migrations
{
  /// <inheritdoc />
  public partial class AddForeignKeyToTransaction : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
          name: "FK_Transaction_TransactionGroup_TransactionGroupId",
          table: "Transaction");

      migrationBuilder.DropForeignKey(
          name: "FK_Transaction_User_UserId",
          table: "Transaction");

      migrationBuilder.AddForeignKey(
          name: "FK_Transaction_TransactionGroup_TransactionGroupId",
          table: "Transaction",
          column: "TransactionGroupId",
          principalTable: "TransactionGroup",
          principalColumn: "Id",
          onDelete: ReferentialAction.SetNull);

      migrationBuilder.AddForeignKey(
          name: "FK_Transaction_User_UserId",
          table: "Transaction",
          column: "UserId",
          principalTable: "User",
          principalColumn: "Id",
          onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
          name: "FK_Transaction_TransactionGroup_TransactionGroupId",
          table: "Transaction");

      migrationBuilder.DropForeignKey(
          name: "FK_Transaction_User_UserId",
          table: "Transaction");

      migrationBuilder.AddForeignKey(
          name: "FK_Transaction_TransactionGroup_TransactionGroupId",
          table: "Transaction",
          column: "TransactionGroupId",
          principalTable: "TransactionGroup",
          principalColumn: "Id");

      migrationBuilder.AddForeignKey(
          name: "FK_Transaction_User_UserId",
          table: "Transaction",
          column: "UserId",
          principalTable: "User",
          principalColumn: "Id",
          onDelete: ReferentialAction.Cascade);
    }
  }
}
