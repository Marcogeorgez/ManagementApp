using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class ChatContainProjectIdONLY : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Users_UserId",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_UserId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Chats");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Chats",
                type: "character varying(255)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chats_ApplicationUserId",
                table: "Chats",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Users_ApplicationUserId",
                table: "Chats",
                column: "ApplicationUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Users_ApplicationUserId",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_ApplicationUserId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Chats");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Chats",
                type: "character varying(255)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_UserId",
                table: "Chats",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Users_UserId",
                table: "Chats",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
