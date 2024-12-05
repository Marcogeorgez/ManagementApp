using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class notRequiredLinkFootage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chats_ProjectId",
                table: "Chats");

            migrationBuilder.AlterColumn<string>(
                name: "footageLink",
                table: "Projects",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Deliverables",
                table: "Projects",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_ProjectId",
                table: "Chats",
                column: "ProjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chats_ProjectId",
                table: "Chats");

            migrationBuilder.AlterColumn<string>(
                name: "footageLink",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Deliverables",
                table: "Projects",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chats_ProjectId",
                table: "Chats",
                column: "ProjectId");
        }
    }
}
