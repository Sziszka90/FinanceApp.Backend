using FinanceApp.Backend.Infrastructure.EntityFramework.Mssql.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Mssql.Migrations;

[DbContext(typeof(FinanceAppMssqlDbContext))]
[Migration("20261023203855_AddBankStatementPrompts")]
public partial class AddBankStatementPrompts : Migration
{
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
      name: "BankStatementPrompt",
      columns: table => new
      {
        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
        Modified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
        Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
        Instructions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
        IsSelected = table.Column<bool>(type: "bit", nullable: false),
        UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_BankStatementPrompt", prompt => prompt.Id);
        table.ForeignKey(
          name: "FK_BankStatementPrompt_User_UserId",
          column: prompt => prompt.UserId,
          principalTable: "User",
          principalColumn: "Id",
          onDelete: ReferentialAction.Cascade);
      });

    migrationBuilder.CreateIndex(
      name: "IX_BankStatementPrompt_UserId_IsSelected",
      table: "BankStatementPrompt",
      columns: new[] { "UserId", "IsSelected" });
  }

  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(name: "BankStatementPrompt");
  }
}
