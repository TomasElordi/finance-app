using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentTradesAndIncome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "auto_price",
                table: "investment_holdings",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "invested_usd",
                table: "investment_holdings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "market_symbol",
                table: "investment_holdings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "realized_gain_usd",
                table: "investment_holdings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "investment_incomes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    holding_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    amount_usd = table.Column<decimal>(type: "numeric", nullable: false),
                    amount_ars = table.Column<decimal>(type: "numeric", nullable: true),
                    usd_rate = table.Column<decimal>(type: "numeric", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_investment_incomes", x => x.id);
                    table.ForeignKey(
                        name: "fk_investment_incomes_investment_holdings_holding_id",
                        column: x => x.holding_id,
                        principalTable: "investment_holdings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "investment_trades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    holding_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    price_ars = table.Column<decimal>(type: "numeric", nullable: true),
                    usd_rate = table.Column<decimal>(type: "numeric", nullable: true),
                    price_usd = table.Column<decimal>(type: "numeric", nullable: false),
                    fee_ars = table.Column<decimal>(type: "numeric", nullable: false),
                    fee_usd = table.Column<decimal>(type: "numeric", nullable: false),
                    is_initial = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_investment_trades", x => x.id);
                    table.ForeignKey(
                        name: "fk_investment_trades_investment_holdings_holding_id",
                        column: x => x.holding_id,
                        principalTable: "investment_holdings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_investment_incomes_holding_id_date",
                table: "investment_incomes",
                columns: new[] { "holding_id", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_investment_incomes_user_id",
                table: "investment_incomes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_investment_trades_holding_id_date",
                table: "investment_trades",
                columns: new[] { "holding_id", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_investment_trades_user_id",
                table: "investment_trades",
                column: "user_id");

            // Quantity and PPC loaded by hand become an initial buy (type 0 = Buy) dated on
            // the migration day (Argentina), so the trade history reproduces them exactly.
            migrationBuilder.Sql("""
                INSERT INTO investment_trades (id, user_id, holding_id, date, type, quantity, price_ars, usd_rate, price_usd, fee_ars, fee_usd, is_initial, created_at)
                SELECT gen_random_uuid(), user_id, id, (now() AT TIME ZONE 'America/Argentina/Buenos_Aires')::date, 0,
                       quantity, NULL, NULL, average_cost_usd, 0, 0, TRUE, now()
                FROM investment_holdings
                WHERE quantity > 0;

                UPDATE investment_holdings SET invested_usd = quantity * average_cost_usd;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "investment_incomes");

            migrationBuilder.DropTable(
                name: "investment_trades");

            migrationBuilder.DropColumn(
                name: "auto_price",
                table: "investment_holdings");

            migrationBuilder.DropColumn(
                name: "invested_usd",
                table: "investment_holdings");

            migrationBuilder.DropColumn(
                name: "market_symbol",
                table: "investment_holdings");

            migrationBuilder.DropColumn(
                name: "realized_gain_usd",
                table: "investment_holdings");
        }
    }
}
