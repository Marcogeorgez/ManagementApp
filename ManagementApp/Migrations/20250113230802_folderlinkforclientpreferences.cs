using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementApp.Migrations;

/// <inheritdoc />
public partial class folderlinkforclientpreferences : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "FolderLink",
            table: "ClientEditingGuidelines",
            type: "text",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FolderLink",
            table: "ClientEditingGuidelines");
    }
}
