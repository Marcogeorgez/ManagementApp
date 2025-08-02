using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementApp.Migrations;

/// <inheritdoc />
public partial class updateRolesName : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TargetRole",
            table: "Notifications",
            type: "text",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TargetRole",
            table: "Notifications");
    }
}
