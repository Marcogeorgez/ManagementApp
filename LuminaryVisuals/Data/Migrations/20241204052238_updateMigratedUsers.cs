using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class updateMigratedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MigratedUsers_Users_UserId",
                table: "MigratedUsers");

            migrationBuilder.DropIndex(
                name: "IX_MigratedUsers_UserId",
                table: "MigratedUsers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MigratedUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "MigratedUsers",
                type: "character varying(255)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MigratedUsers_UserId",
                table: "MigratedUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MigratedUsers_Users_UserId",
                table: "MigratedUsers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
