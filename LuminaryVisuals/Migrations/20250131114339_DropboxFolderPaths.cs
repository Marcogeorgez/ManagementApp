using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations;

/// <inheritdoc />
public partial class DropboxFolderPaths : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PaidFolderPath",
            table: "ClientEditingGuidelines",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PreviewFolderPath",
            table: "ClientEditingGuidelines",
            type: "text",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PaidFolderPath",
            table: "ClientEditingGuidelines");

        migrationBuilder.DropColumn(
            name: "PreviewFolderPath",
            table: "ClientEditingGuidelines");
    }
}
