using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class ProjectCalculationDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CalculationDetails_CameraNumber",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculationDetails_ClientDiscount",
                table: "Projects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CalculationDetails_DocumentaryMulticameraDuration",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CalculationDetails_DocumentaryMulticameraDurationHours",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculationDetails_FinalProjectBillableHours",
                table: "Projects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculationDetails_FootageQuality",
                table: "Projects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculationDetails_FootageSize",
                table: "Projects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculationDetails_HighlightsDifficulty",
                table: "Projects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CalculationDetails_HighlightsDuration",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculationDetails_Misc",
                table: "Projects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CalculationDetails_PrePartsDuration",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculationDetails_PrePartsPrecentage",
                table: "Projects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculationDetails_Resolution",
                table: "Projects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CalculationDetails_SocialMediaDuration",
                table: "Projects",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalculationDetails_CameraNumber",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CalculationDetails_ClientDiscount",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CalculationDetails_DocumentaryMulticameraDuration",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CalculationDetails_DocumentaryMulticameraDurationHours",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CalculationDetails_FinalProjectBillableHours",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CalculationDetails_FootageQuality",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CalculationDetails_FootageSize",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CalculationDetails_HighlightsDifficulty",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CalculationDetails_HighlightsDuration",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CalculationDetails_Misc",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CalculationDetails_PrePartsDuration",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CalculationDetails_PrePartsPrecentage",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CalculationDetails_Resolution",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CalculationDetails_SocialMediaDuration",
                table: "Projects");
        }
    }
}
