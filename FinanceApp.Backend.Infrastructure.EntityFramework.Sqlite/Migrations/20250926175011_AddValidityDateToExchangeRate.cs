using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Infrastructure.EntityFramework.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddValidityDateToExchangeRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "TransactionDate",
                table: "Transaction",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<decimal>(
                name: "ValueInBaseCurrency",
                table: "Transaction",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "Actual",
                table: "ExchangeRate",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ValidFrom",
                table: "ExchangeRate",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ValidTo",
                table: "ExchangeRate",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValueInBaseCurrency",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Actual",
                table: "ExchangeRate");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                table: "ExchangeRate");

            migrationBuilder.DropColumn(
                name: "ValidTo",
                table: "ExchangeRate");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "TransactionDate",
                table: "Transaction",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT");
        }
    }
}
