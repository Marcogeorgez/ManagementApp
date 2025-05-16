using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations;

/// <inheritdoc />
public partial class startEndWorkingTimeForProjectScheduling : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "EndWorkingTime",
            table: "Projects",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "StartWorkingTime",
            table: "Projects",
            type: "timestamp with time zone",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "EndWorkingTime",
            table: "Projects");

        migrationBuilder.DropColumn(
            name: "StartWorkingTime",
            table: "Projects");
    }
}
