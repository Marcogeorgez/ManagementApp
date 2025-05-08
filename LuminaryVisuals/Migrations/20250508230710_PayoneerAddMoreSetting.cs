using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class PayoneerAddMoreSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "PayoneerSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxId",
                table: "PayoneerSettings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "PayoneerSettings");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "PayoneerSettings");
        }
    }
}
