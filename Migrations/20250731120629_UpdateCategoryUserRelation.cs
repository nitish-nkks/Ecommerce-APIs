using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce_APIs.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCategoryUserRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_customers_CreatedById",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_customers_UpdatedById",
                table: "Categories");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByType",
                table: "Categories",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_internal_users_CreatedById",
                table: "Categories",
                column: "CreatedById",
                principalTable: "internal_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_internal_users_UpdatedById",
                table: "Categories",
                column: "UpdatedById",
                principalTable: "internal_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_internal_users_CreatedById",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_internal_users_UpdatedById",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedByType",
                table: "Categories");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_customers_CreatedById",
                table: "Categories",
                column: "CreatedById",
                principalTable: "customers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_customers_UpdatedById",
                table: "Categories",
                column: "UpdatedById",
                principalTable: "customers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
