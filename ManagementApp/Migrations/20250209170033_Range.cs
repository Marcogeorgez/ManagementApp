using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementApp.Migrations;

/// <inheritdoc />
public partial class Range : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "StartWorkingTime",
            table: "Projects",
            newName: "StartDate");

        migrationBuilder.RenameColumn(
            name: "EndWorkingTime",
            table: "Projects",
            newName: "EndDate");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "StartDate",
            table: "Projects",
            newName: "StartWorkingTime");

        migrationBuilder.RenameColumn(
            name: "EndDate",
            table: "Projects",
            newName: "EndWorkingTime");
    }
}
