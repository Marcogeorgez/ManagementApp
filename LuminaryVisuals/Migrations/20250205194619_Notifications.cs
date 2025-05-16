using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LuminaryVisuals.Migrations;

/// <inheritdoc />
public partial class Notifications : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Notifications",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Message = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Notifications", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "UserNotificationStatuses",
            columns: table => new
            {
                UserId = table.Column<int>(type: "integer", nullable: false),
                NotificationId = table.Column<int>(type: "integer", nullable: false),
                Dismissed = table.Column<bool>(type: "boolean", nullable: false),
                DismissedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserNotificationStatuses", x => new { x.NotificationId, x.UserId });
                table.ForeignKey(
                    name: "FK_UserNotificationStatuses_Notifications_NotificationId",
                    column: x => x.NotificationId,
                    principalTable: "Notifications",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserNotificationStatuses_UserId_Dismissed",
            table: "UserNotificationStatuses",
            columns: new[] { "UserId", "Dismissed" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserNotificationStatuses");

        migrationBuilder.DropTable(
            name: "Notifications");
    }
}
