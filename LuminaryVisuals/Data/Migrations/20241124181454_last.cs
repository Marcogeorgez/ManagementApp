using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class last : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EditorFinalBillable",
                table: "Projects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isPaymentVisible",
                table: "Projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditorFinalBillable",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "isPaymentVisible",
                table: "Projects");
        }
    }
}
