using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "balance",
                table: "accounts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            // Backfill: Asset=0 Liability=1 Equity=2 Income=3 Expense=4
            // Debit-normal (Asset, Expense): Debit(1) adds, Credit(0) subtracts
            // Credit-normal (Liability, Equity, Income): Credit(0) adds, Debit(1) subtracts
            migrationBuilder.Sql(@"
                UPDATE accounts
                SET balance = COALESCE((
                    SELECT SUM(
                        CASE
                            WHEN accounts.nature IN (0, 4) THEN
                                CASE WHEN el.type = 1 THEN el.amount ELSE -el.amount END
                            ELSE
                                CASE WHEN el.type = 0 THEN el.amount ELSE -el.amount END
                        END
                    )
                    FROM entry_lines el
                    WHERE el.account_id = accounts.id
                ), 0);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "balance",
                table: "accounts");
        }
    }
}
