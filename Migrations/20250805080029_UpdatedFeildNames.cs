using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce_APIs.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedFeildNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "internal_users",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "customers",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "customer_addresses",
                newName: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "internal_users",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "customers",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "customer_addresses",
                newName: "is_active");
        }
    }
}
