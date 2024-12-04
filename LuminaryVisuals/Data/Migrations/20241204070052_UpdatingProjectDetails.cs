using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingProjectDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Deliverables",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MusicPreference",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PrimaryEditorDetails_AdjustmentHours",
                table: "Projects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrimaryEditorDetails_FinalBillableHours",
                table: "Projects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SecondaryEditorDetails_AdjustmentHours",
                table: "Projects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SecondaryEditorDetails_FinalBillableHours",
                table: "Projects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "footageLink",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deliverables",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "MusicPreference",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PrimaryEditorDetails_AdjustmentHours",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PrimaryEditorDetails_FinalBillableHours",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SecondaryEditorDetails_AdjustmentHours",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SecondaryEditorDetails_FinalBillableHours",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "footageLink",
                table: "Projects");
        }
    }
}
