using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class removingUniqueDateFromEditorLogging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EditorLoggingHours_UserId_ProjectId_Date",
                table: "EditorLoggingHours");

            migrationBuilder.CreateIndex(
                name: "IX_EditorLoggingHours_UserId_ProjectId",
                table: "EditorLoggingHours",
                columns: new[] { "UserId", "ProjectId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EditorLoggingHours_UserId_ProjectId",
                table: "EditorLoggingHours");

            migrationBuilder.CreateIndex(
                name: "IX_EditorLoggingHours_UserId_ProjectId_Date",
                table: "EditorLoggingHours",
                columns: new[] { "UserId", "ProjectId", "Date" },
                unique: true);
        }
    }
}
