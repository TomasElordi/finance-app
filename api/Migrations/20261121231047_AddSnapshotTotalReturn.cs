using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddSnapshotTotalReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "income_usd",
                table: "investment_snapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "invested_usd",
                table: "investment_snapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "realized_gain_usd",
                table: "investment_snapshots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "income_usd",
                table: "investment_snapshots");

            migrationBuilder.DropColumn(
                name: "invested_usd",
                table: "investment_snapshots");

            migrationBuilder.DropColumn(
                name: "realized_gain_usd",
                table: "investment_snapshots");
        }
    }
}
