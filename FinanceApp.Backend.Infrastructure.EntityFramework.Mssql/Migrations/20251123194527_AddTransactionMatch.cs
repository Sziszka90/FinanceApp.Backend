using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Mssql.Migrations
{
  /// <inheritdoc />
  public partial class AddTransactionMatch : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: "MatchTransaction",
        columns: table => new
        {
          Transaction = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
          TransactionGroup = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
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
    }
  }
}
