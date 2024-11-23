using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class AddedClientPaymentAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClientBillable",
                table: "Projects",
                newName: "ClientBillableHours");

            migrationBuilder.AddColumn<decimal>(
                name: "ClientBillableAmount",
                table: "Projects",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientBillableAmount",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "ClientBillableHours",
                table: "Projects",
                newName: "ClientBillable");
        }
    }
}
