using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Infrastructure.EntityFramework.Mssql.Migrations
{
  /// <inheritdoc />
  public partial class Initial : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "Investment",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Value_Currency = table.Column<int>(type: "int", nullable: false),
            Value_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
            Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            Modified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Investment", x => x.Id);
          });

      migrationBuilder.CreateTable(
          name: "Saving",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
            Value_Currency = table.Column<int>(type: "int", nullable: false),
            Value_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            Type = table.Column<int>(type: "int", nullable: false),
            DueDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            Modified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Saving", x => x.Id);
          });

      migrationBuilder.CreateTable(
          name: "User",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
            BaseCurrency = table.Column<int>(type: "int", nullable: false),
            Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            Modified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_User", x => x.Id);
          });

      migrationBuilder.CreateTable(
          name: "TransactionGroup",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
            Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
            UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            GroupType = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: false),
            Limit_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            Limit_Currency = table.Column<int>(type: "int", nullable: true),
            Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            Modified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_TransactionGroup", x => x.Id);
            table.ForeignKey(
                      name: "FK_TransactionGroup_User_UserId",
                      column: x => x.UserId,
                      principalTable: "User",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "ExpenseTransaction",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Priority = table.Column<int>(type: "int", nullable: true),
            Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            Modified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
            Currency = table.Column<int>(type: "int", nullable: false),
            Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            DueDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            TransactionGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ExpenseTransaction", x => x.Id);
            table.ForeignKey(
                      name: "FK_ExpenseTransaction_TransactionGroup_TransactionGroupId",
                      column: x => x.TransactionGroupId,
                      principalTable: "TransactionGroup",
                      principalColumn: "Id");
            table.ForeignKey(
                      name: "FK_ExpenseTransaction_User_UserId",
                      column: x => x.UserId,
                      principalTable: "User",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "IncomeTransaction",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            Modified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
            Currency = table.Column<int>(type: "int", nullable: false),
            Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            DueDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            TransactionGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_IncomeTransaction", x => x.Id);
            table.ForeignKey(
                      name: "FK_IncomeTransaction_TransactionGroup_TransactionGroupId",
                      column: x => x.TransactionGroupId,
                      principalTable: "TransactionGroup",
                      principalColumn: "Id");
            table.ForeignKey(
                      name: "FK_IncomeTransaction_User_UserId",
                      column: x => x.UserId,
                      principalTable: "User",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateIndex(
          name: "IX_ExpenseTransaction_TransactionGroupId",
          table: "ExpenseTransaction",
          column: "TransactionGroupId");

      migrationBuilder.CreateIndex(
          name: "IX_ExpenseTransaction_UserId",
          table: "ExpenseTransaction",
          column: "UserId");

      migrationBuilder.CreateIndex(
          name: "IX_IncomeTransaction_TransactionGroupId",
          table: "IncomeTransaction",
          column: "TransactionGroupId");

      migrationBuilder.CreateIndex(
          name: "IX_IncomeTransaction_UserId",
          table: "IncomeTransaction",
          column: "UserId");

      migrationBuilder.CreateIndex(
          name: "IX_TransactionGroup_UserId",
          table: "TransactionGroup",
          column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "ExpenseTransaction");

      migrationBuilder.DropTable(
          name: "IncomeTransaction");

      migrationBuilder.DropTable(
          name: "Investment");

      migrationBuilder.DropTable(
          name: "Saving");

      migrationBuilder.DropTable(
          name: "TransactionGroup");

      migrationBuilder.DropTable(
          name: "User");
    }
  }
}
