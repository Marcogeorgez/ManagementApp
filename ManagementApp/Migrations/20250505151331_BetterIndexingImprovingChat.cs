using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementApp.Migrations;

/// <inheritdoc />
public partial class BetterIndexingImprovingChat : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_UserProjectPins_UserId_IsPinned",
            table: "UserProjectPins",
            columns: new[] { "UserId", "IsPinned" });

        migrationBuilder.CreateIndex(
            name: "IX_Projects_InternalOrder",
            table: "Projects",
            column: "InternalOrder");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_IsArchived",
            table: "Projects",
            column: "IsArchived");

        migrationBuilder.CreateIndex(
            name: "IX_Projects_IsArchived_InternalOrder",
            table: "Projects",
            columns: new[] { "IsArchived", "InternalOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_Projects_IsArchived_PrimaryEditorId_SecondaryEditorId",
            table: "Projects",
            columns: new[] { "IsArchived", "PrimaryEditorId", "SecondaryEditorId" });

        migrationBuilder.CreateIndex(
            name: "IX_Messages_IsApproved_IsDeleted",
            table: "Messages",
            columns: new[] { "IsApproved", "IsDeleted" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_UserProjectPins_UserId_IsPinned",
            table: "UserProjectPins");

        migrationBuilder.DropIndex(
            name: "IX_Projects_InternalOrder",
            table: "Projects");

        migrationBuilder.DropIndex(
            name: "IX_Projects_IsArchived",
            table: "Projects");

        migrationBuilder.DropIndex(
            name: "IX_Projects_IsArchived_InternalOrder",
            table: "Projects");

        migrationBuilder.DropIndex(
            name: "IX_Projects_IsArchived_PrimaryEditorId_SecondaryEditorId",
            table: "Projects");

        migrationBuilder.DropIndex(
            name: "IX_Messages_IsApproved_IsDeleted",
            table: "Messages");
    }
}
