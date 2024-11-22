using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class InternalExternalOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExternalOrder",
                table: "Projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InternalOrder",
                table: "Projects",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalOrder",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "InternalOrder",
                table: "Projects");
        }
    }
}
