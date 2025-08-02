using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ManagementApp.Migrations;

/// <inheritdoc />
public partial class updatePin : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_UserProjectPins_Projects_ProjectId",
            table: "UserProjectPins");

        migrationBuilder.DropForeignKey(
            name: "FK_UserProjectPins_Users_UserId",
            table: "UserProjectPins");

        migrationBuilder.DropPrimaryKey(
            name: "PK_UserProjectPins",
            table: "UserProjectPins");

        migrationBuilder.DropIndex(
            name: "IX_UserProjectPins_UserId_IsPinned",
            table: "UserProjectPins");

        migrationBuilder.RenameTable(
            name: "UserProjectPins",
            newName: "UserChatPins");

        migrationBuilder.RenameIndex(
            name: "IX_UserProjectPins_ProjectId",
            table: "UserChatPins",
            newName: "IX_UserChatPins_ProjectId");

        migrationBuilder.AlterColumn<int>(
            name: "ProjectId",
            table: "UserChatPins",
            type: "integer",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "integer");

        migrationBuilder.AddColumn<int>(
            name: "Id",
            table: "UserChatPins",
            type: "integer",
            nullable: false,
            defaultValue: 0)
            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

        migrationBuilder.AddColumn<string>(
            name: "UserChatId",
            table: "UserChatPins",
            type: "text",
            nullable: true);

        migrationBuilder.AddPrimaryKey(
            name: "PK_UserChatPins",
            table: "UserChatPins",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_UserChatPins_UserId",
            table: "UserChatPins",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_UserChatPins_Projects_ProjectId",
            table: "UserChatPins",
            column: "ProjectId",
            principalTable: "Projects",
            principalColumn: "ProjectId");

        migrationBuilder.AddForeignKey(
            name: "FK_UserChatPins_Users_UserId",
            table: "UserChatPins",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_UserChatPins_Projects_ProjectId",
            table: "UserChatPins");

        migrationBuilder.DropForeignKey(
            name: "FK_UserChatPins_Users_UserId",
            table: "UserChatPins");

        migrationBuilder.DropPrimaryKey(
            name: "PK_UserChatPins",
            table: "UserChatPins");

        migrationBuilder.DropIndex(
            name: "IX_UserChatPins_UserId",
            table: "UserChatPins");

        migrationBuilder.DropColumn(
            name: "Id",
            table: "UserChatPins");

        migrationBuilder.DropColumn(
            name: "UserChatId",
            table: "UserChatPins");

        migrationBuilder.RenameTable(
            name: "UserChatPins",
            newName: "UserProjectPins");

        migrationBuilder.RenameIndex(
            name: "IX_UserChatPins_ProjectId",
            table: "UserProjectPins",
            newName: "IX_UserProjectPins_ProjectId");

        migrationBuilder.AlterColumn<int>(
            name: "ProjectId",
            table: "UserProjectPins",
            type: "integer",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "integer",
            oldNullable: true);

        migrationBuilder.AddPrimaryKey(
            name: "PK_UserProjectPins",
            table: "UserProjectPins",
            columns: new[] { "UserId", "ProjectId" });

        migrationBuilder.CreateIndex(
            name: "IX_UserProjectPins_UserId_IsPinned",
            table: "UserProjectPins",
            columns: new[] { "UserId", "IsPinned" });

        migrationBuilder.AddForeignKey(
            name: "FK_UserProjectPins_Projects_ProjectId",
            table: "UserProjectPins",
            column: "ProjectId",
            principalTable: "Projects",
            principalColumn: "ProjectId",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_UserProjectPins_Users_UserId",
            table: "UserProjectPins",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
