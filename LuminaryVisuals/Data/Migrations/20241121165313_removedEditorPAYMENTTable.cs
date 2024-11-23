using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class removedEditorPAYMENTTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EditorPayments");

            migrationBuilder.AddColumn<DateTime>(
                name: "EditorDatePaid",
                table: "Projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EditorOvertime",
                table: "Projects",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EditorPaymentAmount",
                table: "Projects",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EditorWorkingHours",
                table: "Projects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isEditorPaid",
                table: "Projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditorDatePaid",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "EditorOvertime",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "EditorPaymentAmount",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "EditorWorkingHours",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "isEditorPaid",
                table: "Projects");

            migrationBuilder.CreateTable(
                name: "EditorPayments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(255)", nullable: false),
                    EditorDatePaid = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Overtime = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    PaymentAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    WorkingHours = table.Column<decimal>(type: "numeric", nullable: true),
                    isPaid = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorPayments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_EditorPayments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EditorPayments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EditorPayments_ProjectId",
                table: "EditorPayments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_EditorPayments_UserId",
                table: "EditorPayments",
                column: "UserId");
        }
    }
}
