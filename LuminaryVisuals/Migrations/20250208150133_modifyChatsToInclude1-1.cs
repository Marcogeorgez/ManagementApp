using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class modifyChatsToInclude11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Users_ApplicationUserId",
                table: "Chats");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Chats",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Chats_ApplicationUserId",
                table: "Chats",
                newName: "IX_Chats_UserId");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "Chats",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdminChat",
                table: "Chats",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Users_UserId",
                table: "Chats",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Users_UserId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "IsAdminChat",
                table: "Chats");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Chats",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Chats_UserId",
                table: "Chats",
                newName: "IX_Chats_ApplicationUserId");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "Chats",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Users_ApplicationUserId",
                table: "Chats",
                column: "ApplicationUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
