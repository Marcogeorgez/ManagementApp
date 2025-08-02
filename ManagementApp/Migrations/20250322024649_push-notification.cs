using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementApp.Migrations;

/// <inheritdoc />
public partial class pushnotification : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PushAuth",
            table: "Users",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "PushEndpoint",
            table: "Users",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "PushP256DH",
            table: "Users",
            type: "text",
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
