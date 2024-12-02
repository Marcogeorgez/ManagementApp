using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class ProjectSpecification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProjectSpecifications_CameraNumber",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjectSpecifications_ColorProfile",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjectSpecifications_Resolution",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjectSpecifications_Size",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectSpecifications_CameraNumber",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProjectSpecifications_ColorProfile",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProjectSpecifications_Resolution",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProjectSpecifications_Size",
                table: "Projects");
        }
    }
}
