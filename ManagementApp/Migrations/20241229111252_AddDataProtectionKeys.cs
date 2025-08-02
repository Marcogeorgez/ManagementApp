using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementApp.Migrations;

/// <inheritdoc />
public partial class AddDataProtectionKeys : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DataProtectionKeys",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                FriendlyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Xml = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DataProtectionKeys");
    }
}
