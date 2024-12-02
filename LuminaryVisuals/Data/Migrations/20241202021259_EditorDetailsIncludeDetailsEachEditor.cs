using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class EditorDetailsIncludeDetailsEachEditor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditorOvertime",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "isEditorPaid",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "BillableHours",
                table: "Projects",
                newName: "SecondaryEditorDetails_BillableHours");

            migrationBuilder.RenameColumn(
                name: "EditorPaymentAmount",
                table: "Projects",
                newName: "SecondaryEditorDetails_PaymentAmount");

            migrationBuilder.RenameColumn(
                name: "EditorFinalBillable",
                table: "Projects",
                newName: "SecondaryEditorDetails_Overtime");

            migrationBuilder.AddColumn<decimal>(
                name: "PrimaryEditorDetails_BillableHours",
                table: "Projects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrimaryEditorDetails_Overtime",
                table: "Projects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrimaryEditorDetails_PaymentAmount",
                table: "Projects",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryEditorDetails_BillableHours",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PrimaryEditorDetails_Overtime",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PrimaryEditorDetails_PaymentAmount",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "SecondaryEditorDetails_BillableHours",
                table: "Projects",
                newName: "BillableHours");

            migrationBuilder.RenameColumn(
                name: "SecondaryEditorDetails_PaymentAmount",
                table: "Projects",
                newName: "EditorPaymentAmount");

            migrationBuilder.RenameColumn(
                name: "SecondaryEditorDetails_Overtime",
                table: "Projects",
                newName: "EditorFinalBillable");

            migrationBuilder.AddColumn<decimal>(
                name: "EditorOvertime",
                table: "Projects",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isEditorPaid",
                table: "Projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
