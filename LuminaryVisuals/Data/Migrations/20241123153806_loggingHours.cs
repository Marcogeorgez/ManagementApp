using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class loggingHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditorDatePaid",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "EditorWorkingHours",
                table: "Projects");

            migrationBuilder.AlterColumn<decimal>(
                name: "EditorPaymentAmount",
                table: "Projects",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "EditorLoggingHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(255)", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    EditorWorkingHours = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorLoggingHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EditorLoggingHours_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EditorLoggingHours_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EditorLoggingHours_ProjectId",
                table: "EditorLoggingHours",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_EditorLoggingHours_UserId_ProjectId_Date",
                table: "EditorLoggingHours",
                columns: new[] { "UserId", "ProjectId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EditorLoggingHours");

            migrationBuilder.AlterColumn<decimal>(
                name: "EditorPaymentAmount",
                table: "Projects",
                type: "numeric(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EditorDatePaid",
                table: "Projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EditorWorkingHours",
                table: "Projects",
                type: "numeric",
                nullable: true);
        }
    }
}
