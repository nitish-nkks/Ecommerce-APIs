using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce_APIs.Migrations
{
    /// <inheritdoc />
    public partial class AddedminmaxQuntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinOrderQuantity",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinOrderQuantity",
                table: "Products");
        }
    }
}
