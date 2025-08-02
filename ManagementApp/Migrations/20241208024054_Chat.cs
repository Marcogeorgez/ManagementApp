using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ManagementApp.Migrations;

/// <inheritdoc />
public partial class Chat : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ChatReadStatus_Chats_ChatId",
            table: "ChatReadStatus");

        migrationBuilder.DropColumn(
            name: "IsApproved",
            table: "Chats");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Chats");

        migrationBuilder.DropColumn(
            name: "IsEditorMessage",
            table: "Chats");

        migrationBuilder.DropColumn(
            name: "Message",
            table: "Chats");

        migrationBuilder.DropColumn(
            name: "Timestamp",
            table: "Chats");

        migrationBuilder.RenameColumn(
            name: "ChatId",
            table: "ChatReadStatus",
            newName: "MessageId");

        migrationBuilder.RenameIndex(
            name: "IX_ChatReadStatus_ChatId_UserId",
            table: "ChatReadStatus",
            newName: "IX_ChatReadStatus_MessageId_UserId");

        migrationBuilder.CreateTable(
            name: "Messages",
            columns: table => new
            {
                MessageId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ChatId = table.Column<int>(type: "integer", nullable: false),
                UserId = table.Column<string>(type: "character varying(255)", nullable: false),
                Content = table.Column<string>(type: "text", nullable: false),
                Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsApproved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Messages", x => x.MessageId);
                table.ForeignKey(
                    name: "FK_Messages_Chats_ChatId",
                    column: x => x.ChatId,
                    principalTable: "Chats",
                    principalColumn: "ChatId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Messages_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Messages_ChatId",
            table: "Messages",
            column: "ChatId");

        migrationBuilder.CreateIndex(
            name: "IX_Messages_UserId",
            table: "Messages",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_ChatReadStatus_Messages_MessageId",
            table: "ChatReadStatus",
            column: "MessageId",
            principalTable: "Messages",
            principalColumn: "MessageId",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ChatReadStatus_Messages_MessageId",
            table: "ChatReadStatus");

        migrationBuilder.DropTable(
            name: "Messages");

        migrationBuilder.RenameColumn(
            name: "MessageId",
            table: "ChatReadStatus",
            newName: "ChatId");

        migrationBuilder.RenameIndex(
            name: "IX_ChatReadStatus_MessageId_UserId",
            table: "ChatReadStatus",
            newName: "IX_ChatReadStatus_ChatId_UserId");

        migrationBuilder.AddColumn<bool>(
            name: "IsApproved",
            table: "Chats",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Chats",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsEditorMessage",
            table: "Chats",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "Message",
            table: "Chats",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<DateTime>(
            name: "Timestamp",
            table: "Chats",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddForeignKey(
            name: "FK_ChatReadStatus_Chats_ChatId",
            table: "ChatReadStatus",
            column: "ChatId",
            principalTable: "Chats",
            principalColumn: "ChatId",
            onDelete: ReferentialAction.Cascade);
    }
}
