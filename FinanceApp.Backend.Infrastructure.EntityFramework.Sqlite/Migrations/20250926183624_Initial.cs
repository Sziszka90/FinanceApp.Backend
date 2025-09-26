using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite.Migrations
{
  /// <inheritdoc />
  public partial class Initial : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "ExchangeRate",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "TEXT", nullable: false),
            BaseCurrency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
            TargetCurrency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
            Rate = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
            Actual = table.Column<bool>(type: "INTEGER", nullable: false),
            ValidFrom = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            ValidTo = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
            Created = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            Modified = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ExchangeRate", x => x.Id);
          });

      migrationBuilder.CreateTable(
          name: "User",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "TEXT", nullable: false),
            UserName = table.Column<string>(type: "TEXT", nullable: false),
            Email = table.Column<string>(type: "TEXT", nullable: false),
            IsEmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
            PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
            BaseCurrency = table.Column<int>(type: "INTEGER", nullable: false),
            ResetPasswordToken = table.Column<string>(type: "TEXT", nullable: true),
            ResetPasswordTokenExpiration = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
            EmailConfirmationToken = table.Column<string>(type: "TEXT", nullable: true),
            EmailConfirmationTokenExpiration = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
            Created = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            Modified = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_User", x => x.Id);
          });

      migrationBuilder.CreateTable(
          name: "TransactionGroup",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "TEXT", nullable: false),
            Name = table.Column<string>(type: "TEXT", nullable: false),
            Description = table.Column<string>(type: "TEXT", nullable: true),
            GroupIcon = table.Column<string>(type: "TEXT", nullable: true),
            UserId = table.Column<Guid>(type: "TEXT", nullable: false),
            Created = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            Modified = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
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
          name: "Transaction",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "TEXT", nullable: false),
            Name = table.Column<string>(type: "TEXT", nullable: false),
            Description = table.Column<string>(type: "TEXT", nullable: true),
            TransactionType = table.Column<int>(type: "INTEGER", nullable: true),
            Currency = table.Column<int>(type: "INTEGER", nullable: false),
            Amount = table.Column<decimal>(type: "TEXT", nullable: false),
            ValueInBaseCurrency = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
            TransactionGroupId = table.Column<Guid>(type: "TEXT", nullable: true),
            UserId = table.Column<Guid>(type: "TEXT", nullable: false),
            TransactionDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            Created = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
            Modified = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Transaction", x => x.Id);
            table.ForeignKey(
                      name: "FK_Transaction_TransactionGroup_TransactionGroupId",
                      column: x => x.TransactionGroupId,
                      principalTable: "TransactionGroup",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.SetNull);
            table.ForeignKey(
                      name: "FK_Transaction_User_UserId",
                      column: x => x.UserId,
                      principalTable: "User",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Restrict);
          });

      migrationBuilder.CreateIndex(
          name: "IX_Transaction_TransactionGroupId",
          table: "Transaction",
          column: "TransactionGroupId");

      migrationBuilder.CreateIndex(
          name: "IX_Transaction_UserId",
          table: "Transaction",
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
          name: "ExchangeRate");

      migrationBuilder.DropTable(
          name: "Transaction");

      migrationBuilder.DropTable(
          name: "TransactionGroup");

      migrationBuilder.DropTable(
          name: "User");
    }
  }
}
