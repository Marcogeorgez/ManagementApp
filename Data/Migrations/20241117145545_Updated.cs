using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class Updated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EditorPayments_UserId_ProjectId_PaymentMonth_PaymentYear",
                table: "EditorPayments");

            migrationBuilder.DropColumn(
                name: "BillableHours",
                table: "EditorPayments");

            migrationBuilder.DropColumn(
                name: "PaymentMonth",
                table: "EditorPayments");

            migrationBuilder.DropColumn(
                name: "PaymentYear",
                table: "EditorPayments");

            migrationBuilder.DropColumn(
                name: "Undertime",
                table: "EditorPayments");

            migrationBuilder.AddColumn<decimal>(
                name: "BillableHours",
                table: "Projects",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "WorkingHours",
                table: "EditorPayments",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)");

            migrationBuilder.CreateIndex(
                name: "IX_EditorPayments_UserId",
                table: "EditorPayments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EditorPayments_UserId",
                table: "EditorPayments");

            migrationBuilder.DropColumn(
                name: "BillableHours",
                table: "Projects");

            migrationBuilder.AlterColumn<decimal>(
                name: "WorkingHours",
                table: "EditorPayments",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<decimal>(
                name: "BillableHours",
                table: "EditorPayments",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMonth",
                table: "EditorPayments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentYear",
                table: "EditorPayments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Undertime",
                table: "EditorPayments",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_EditorPayments_UserId_ProjectId_PaymentMonth_PaymentYear",
                table: "EditorPayments",
                columns: new[] { "UserId", "ProjectId", "PaymentMonth", "PaymentYear" },
                unique: true);
        }
    }
}
