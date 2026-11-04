using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddCardItemRecurrence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "end_month",
                table: "card_items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "end_year",
                table: "card_items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_recurring",
                table: "card_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "end_month",
                table: "card_items");

            migrationBuilder.DropColumn(
                name: "end_year",
                table: "card_items");

            migrationBuilder.DropColumn(
                name: "is_recurring",
                table: "card_items");
        }
    }
}
