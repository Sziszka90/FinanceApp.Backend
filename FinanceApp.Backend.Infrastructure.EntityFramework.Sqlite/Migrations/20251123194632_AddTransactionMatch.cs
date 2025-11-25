using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite.Migrations
{
  /// <inheritdoc />
  public partial class AddTransactionMatch : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<decimal>(
        name: "Amount",
        table: "Transaction",
        type: "decimal(18,4)",
        nullable: false,
        oldClrType: typeof(decimal),
        oldType: "TEXT");

      migrationBuilder.CreateTable(
        name: "MatchTransaction",
        columns: table => new
        {
          Transaction = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
          TransactionGroup = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_MatchTransaction", x => new { x.Transaction, x.TransactionGroup });
        });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
        name: "MatchTransaction");

      migrationBuilder.AlterColumn<decimal>(
        name: "Amount",
        table: "Transaction",
        type: "TEXT",
        nullable: false,
        oldClrType: typeof(decimal),
        oldType: "decimal(18,4)");
    }
  }
}
