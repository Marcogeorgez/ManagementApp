using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class RemovedVideoEditorsAndAddedPrimarySecondaryEditorsToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoEditors");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryEditorId",
                table: "Projects",
                type: "text",
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryEditorId",
                table: "Projects",
                type: "text",
                nullable: true,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryEditorId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SecondaryEditorId",
                table: "Projects");

            migrationBuilder.CreateTable(
                name: "VideoEditors",
                columns: table => new
                {
                    VideoEditorId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(255)", nullable: false),
                    Label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoEditors", x => x.VideoEditorId);
                    table.ForeignKey(
                        name: "FK_VideoEditors_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoEditors_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoEditors_ProjectId",
                table: "VideoEditors",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoEditors_UserId",
                table: "VideoEditors",
                column: "UserId");
        }
    }
}
