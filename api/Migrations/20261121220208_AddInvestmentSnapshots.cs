using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "allow_fractional",
                table: "investment_groups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "investment_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    usd_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    value_usd = table.Column<decimal>(type: "numeric", nullable: false),
                    cost_usd = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_investment_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "fk_investment_snapshots_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "investment_snapshot_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    value_usd = table.Column<decimal>(type: "numeric", nullable: false),
                    cost_usd = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_investment_snapshot_groups", x => x.id);
                    table.ForeignKey(
                        name: "fk_investment_snapshot_groups_investment_snapshots_snapshot_id",
                        column: x => x.snapshot_id,
                        principalTable: "investment_snapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_investment_snapshot_groups_snapshot_id",
                table: "investment_snapshot_groups",
                column: "snapshot_id");

            migrationBuilder.CreateIndex(
                name: "ix_investment_snapshots_user_id_date",
                table: "investment_snapshots",
                columns: new[] { "user_id", "date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "investment_snapshot_groups");

            migrationBuilder.DropTable(
                name: "investment_snapshots");

            migrationBuilder.DropColumn(
                name: "allow_fractional",
                table: "investment_groups");
        }
    }
}
