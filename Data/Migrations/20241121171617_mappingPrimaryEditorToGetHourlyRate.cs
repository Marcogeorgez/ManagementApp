using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class mappingPrimaryEditorToGetHourlyRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SecondaryEditorId",
                table: "Projects",
                type: "character varying(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryEditorId",
                table: "Projects",
                type: "character varying(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_PrimaryEditorId",
                table: "Projects",
                column: "PrimaryEditorId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_SecondaryEditorId",
                table: "Projects",
                column: "SecondaryEditorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_PrimaryEditorId",
                table: "Projects",
                column: "PrimaryEditorId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_SecondaryEditorId",
                table: "Projects",
                column: "SecondaryEditorId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_PrimaryEditorId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_SecondaryEditorId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_PrimaryEditorId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_SecondaryEditorId",
                table: "Projects");

            migrationBuilder.AlterColumn<string>(
                name: "SecondaryEditorId",
                table: "Projects",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryEditorId",
                table: "Projects",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldNullable: true);
        }
    }
}
