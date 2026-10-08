using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddEntryIsClosing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_closing",
                table: "entries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Mark closing entries created before this column existed
            migrationBuilder.Sql(
                "UPDATE entries SET is_closing = true WHERE title LIKE 'Cierre de resultados %';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_closing",
                table: "entries");
        }
    }
}
