using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Infrastructure.EntityFramework.Mssql.Migrations
{
    /// <inheritdoc />
    public partial class AddValidityDateToExchangeRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ValueInBaseCurrency",
                table: "Transaction",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "Actual",
                table: "ExchangeRate",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ValidFrom",
                table: "ExchangeRate",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ValidTo",
                table: "ExchangeRate",
                type: "datetimeoffset",
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
        }
    }
}
