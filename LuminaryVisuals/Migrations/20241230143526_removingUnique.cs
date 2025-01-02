using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class removingUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EditorLoggingHours_UserId_ProjectId",
                table: "EditorLoggingHours");

            migrationBuilder.CreateIndex(
                name: "IX_EditorLoggingHours_UserId",
                table: "EditorLoggingHours",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EditorLoggingHours_UserId",
                table: "EditorLoggingHours");

            migrationBuilder.CreateIndex(
                name: "IX_EditorLoggingHours_UserId_ProjectId",
                table: "EditorLoggingHours",
                columns: new[] { "UserId", "ProjectId" },
                unique: true);
        }
    }
}
