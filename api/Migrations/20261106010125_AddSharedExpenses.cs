using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shared_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    invite_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shared_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_shared_categories_shared_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "shared_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shared_group_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_group_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_shared_group_members_shared_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "shared_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shared_group_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shared_settlements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    from_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    note = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_settlements", x => x.id);
                    table.ForeignKey(
                        name: "fk_shared_settlements_shared_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "shared_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shared_settlements_users_from_user_id",
                        column: x => x.from_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_shared_settlements_users_to_user_id",
                        column: x => x.to_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shared_expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    total = table.Column<decimal>(type: "numeric", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    paid_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_expenses", x => x.id);
                    table.ForeignKey(
                        name: "fk_shared_expenses_shared_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "shared_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_shared_expenses_shared_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "shared_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shared_expenses_users_paid_by_user_id",
                        column: x => x.paid_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shared_expense_splits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    expense_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_expense_splits", x => x.id);
                    table.ForeignKey(
                        name: "fk_shared_expense_splits_shared_expenses_expense_id",
                        column: x => x.expense_id,
                        principalTable: "shared_expenses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shared_expense_splits_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_shared_categories_group_id",
                table: "shared_categories",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_expense_splits_expense_id_user_id",
                table: "shared_expense_splits",
                columns: new[] { "expense_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shared_expense_splits_user_id",
                table: "shared_expense_splits",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_expenses_category_id",
                table: "shared_expenses",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_expenses_group_id_date",
                table: "shared_expenses",
                columns: new[] { "group_id", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_shared_expenses_paid_by_user_id",
                table: "shared_expenses",
                column: "paid_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_group_members_group_id_user_id",
                table: "shared_group_members",
                columns: new[] { "group_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shared_group_members_user_id",
                table: "shared_group_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_groups_invite_code",
                table: "shared_groups",
                column: "invite_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shared_settlements_from_user_id",
                table: "shared_settlements",
                column: "from_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_settlements_group_id_date",
                table: "shared_settlements",
                columns: new[] { "group_id", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_shared_settlements_to_user_id",
                table: "shared_settlements",
                column: "to_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shared_expense_splits");

            migrationBuilder.DropTable(
                name: "shared_group_members");

            migrationBuilder.DropTable(
                name: "shared_settlements");

            migrationBuilder.DropTable(
                name: "shared_expenses");

            migrationBuilder.DropTable(
                name: "shared_categories");

            migrationBuilder.DropTable(
                name: "shared_groups");
        }
    }
}
