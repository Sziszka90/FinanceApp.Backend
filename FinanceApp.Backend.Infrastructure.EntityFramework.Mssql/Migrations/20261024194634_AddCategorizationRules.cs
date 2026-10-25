using FinanceApp.Backend.Infrastructure.EntityFramework.Mssql.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Mssql.Migrations;

[DbContext(typeof(FinanceAppMssqlDbContext))]
[Migration("20261024194634_AddCategorizationRules")]
public partial class AddCategorizationRules : Migration
{
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
      name: "CategorizationRule",
      columns: table => new
      {
        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        MerchantText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
        TransactionGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
        Modified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_CategorizationRule", x => x.Id);
        table.ForeignKey("FK_CategorizationRule_TransactionGroup_TransactionGroupId", x => x.TransactionGroupId,
          "TransactionGroup", "Id", onDelete: ReferentialAction.Cascade);
        table.ForeignKey("FK_CategorizationRule_User_UserId", x => x.UserId, "User", "Id", onDelete: ReferentialAction.Restrict);
      });

    migrationBuilder.CreateIndex("IX_CategorizationRule_TransactionGroupId", "CategorizationRule", "TransactionGroupId");
    migrationBuilder.CreateIndex("IX_CategorizationRule_UserId", "CategorizationRule", "UserId");
  }

  protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("CategorizationRule");
}
