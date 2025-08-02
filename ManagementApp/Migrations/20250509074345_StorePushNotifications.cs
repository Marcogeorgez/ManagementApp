using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ManagementApp.Migrations;

/// <inheritdoc />
public partial class StorePushNotifications : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PushAuth",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "PushEndpoint",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "PushP256DH",
            table: "Users");

        migrationBuilder.CreateTable(
            name: "PushNotificationSubscriptions",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<string>(type: "character varying(255)", nullable: false),
                Endpoint = table.Column<string>(type: "text", nullable: false),
                P256DH = table.Column<string>(type: "text", nullable: false),
                Auth = table.Column<string>(type: "text", nullable: false),
                Status = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PushNotificationSubscriptions", x => x.Id);
                table.ForeignKey(
                    name: "FK_PushNotificationSubscriptions_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PushNotificationSubscriptions_UserId",
            table: "PushNotificationSubscriptions",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PushNotificationSubscriptions");

        migrationBuilder.AddColumn<string>(
            name: "PushAuth",
            table: "Users",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PushEndpoint",
            table: "Users",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PushP256DH",
            table: "Users",
            type: "text",
            nullable: true);
    }
}
