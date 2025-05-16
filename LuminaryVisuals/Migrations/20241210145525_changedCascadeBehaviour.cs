using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations;

/// <inheritdoc />
public partial class changedCascadeBehaviour : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ChatReadStatus_Users_UserId",
            table: "ChatReadStatus");

        migrationBuilder.AddForeignKey(
            name: "FK_ChatReadStatus_Users_UserId",
            table: "ChatReadStatus",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ChatReadStatus_Users_UserId",
            table: "ChatReadStatus");

        migrationBuilder.AddForeignKey(
            name: "FK_ChatReadStatus_Users_UserId",
            table: "ChatReadStatus",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}
