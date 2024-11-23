using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class ClientEditingGuidelinesOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isPaid",
                table: "EditorPayments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ClientEditingGuidelines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(255)", nullable: false),
                    WebsiteLink = table.Column<string>(type: "text", nullable: false),
                    HighlightsFilm = table.Column<bool>(type: "boolean", nullable: false),
                    VideoStructure = table.Column<string>(type: "text", nullable: false),
                    CrossFades = table.Column<string>(type: "text", nullable: false),
                    FadeToBlack = table.Column<string>(type: "text", nullable: false),
                    BlackAndWhite = table.Column<string>(type: "text", nullable: false),
                    DoubleExposure = table.Column<string>(type: "text", nullable: false),
                    MaskingTransitions = table.Column<string>(type: "text", nullable: false),
                    LensFlares = table.Column<string>(type: "text", nullable: false),
                    OldFilmLook = table.Column<string>(type: "text", nullable: false),
                    PictureInPicture = table.Column<string>(type: "text", nullable: false),
                    OtherTransitions = table.Column<string>(type: "text", nullable: false),
                    TransitionComments = table.Column<string>(type: "text", nullable: false),
                    UseSpeeches = table.Column<string>(type: "text", nullable: false),
                    SpeechComments = table.Column<string>(type: "text", nullable: false),
                    SoundDesignEmphasis = table.Column<string>(type: "text", nullable: false),
                    SoundDesignComments = table.Column<string>(type: "text", nullable: false),
                    MusicGenresArtists = table.Column<string>(type: "text", nullable: false),
                    SongSamples = table.Column<string>(type: "text", nullable: false),
                    ColorReferences = table.Column<string>(type: "text", nullable: false),
                    FilmReferences = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientEditingGuidelines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientEditingGuidelines_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientEditingGuidelines_UserId",
                table: "ClientEditingGuidelines",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientEditingGuidelines");

            migrationBuilder.DropColumn(
                name: "isPaid",
                table: "EditorPayments");
        }
    }
}
