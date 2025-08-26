using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce_APIs.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedFlashSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "FlashSales",
                type: "datetime(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "FlashSales",
                type: "datetime(3)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "FlashSales",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaleName",
                table: "FlashSales",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "FlashSales",
                type: "time(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "FlashSales");

            migrationBuilder.DropColumn(
                name: "SaleName",
                table: "FlashSales");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "FlashSales");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "FlashSales",
                type: "datetime(3)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "FlashSales",
                type: "datetime(3)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldNullable: true);
        }
    }
}
