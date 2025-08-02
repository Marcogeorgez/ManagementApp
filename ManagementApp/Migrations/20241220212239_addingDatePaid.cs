using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManagementApp.Migrations;

/// <inheritdoc />
public partial class addingDatePaid : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "PrimaryEditorDetails_DatePaidEditor",
            table: "Projects",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "SecondaryEditorDetails_DatePaidEditor",
            table: "Projects",
            type: "timestamp with time zone",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PrimaryEditorDetails_DatePaidEditor",
            table: "Projects");

        migrationBuilder.DropColumn(
            name: "SecondaryEditorDetails_DatePaidEditor",
            table: "Projects");
    }
}
