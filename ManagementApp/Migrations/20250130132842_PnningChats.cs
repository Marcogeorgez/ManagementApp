using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementApp.Migrations;

/// <inheritdoc />
public partial class PnningChats : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserProjectPins",
            columns: table => new
            {
                UserId = table.Column<string>(type: "character varying(255)", nullable: false),
                ProjectId = table.Column<int>(type: "integer", nullable: false),
                IsPinned = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserProjectPins", x => new { x.UserId, x.ProjectId });
                table.ForeignKey(
                    name: "FK_UserProjectPins_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "ProjectId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserProjectPins_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserProjectPins_ProjectId",
            table: "UserProjectPins",
            column: "ProjectId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserProjectPins");
    }
}
