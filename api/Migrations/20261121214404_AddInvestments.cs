using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "investment_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_investment_groups", x => x.id);
                    table.ForeignKey(
                        name: "fk_investment_groups_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "investment_portfolios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_amount_usd = table.Column<decimal>(type: "numeric", nullable: false),
                    usd_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_investment_portfolios", x => x.id);
                    table.ForeignKey(
                        name: "fk_investment_portfolios_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "investment_holdings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticker = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    target_percent = table.Column<decimal>(type: "numeric", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    average_cost_usd = table.Column<decimal>(type: "numeric", nullable: false),
                    price_ars = table.Column<decimal>(type: "numeric", nullable: false),
                    price_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_investment_holdings", x => x.id);
                    table.ForeignKey(
                        name: "fk_investment_holdings_investment_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "investment_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_investment_holdings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_investment_groups_user_id",
                table: "investment_groups",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_investment_holdings_group_id",
                table: "investment_holdings",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_investment_holdings_user_id_ticker",
                table: "investment_holdings",
                columns: new[] { "user_id", "ticker" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_investment_portfolios_user_id",
                table: "investment_portfolios",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "investment_holdings");

            migrationBuilder.DropTable(
                name: "investment_portfolios");

            migrationBuilder.DropTable(
                name: "investment_groups");
        }
    }
}
