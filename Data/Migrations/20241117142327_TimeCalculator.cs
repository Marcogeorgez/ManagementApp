using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class TimeCalculator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalculationParameter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ParameterType = table.Column<string>(type: "text", nullable: false),
                    DefaultValue = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculationParameter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalculationOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CalculationParameterId = table.Column<int>(type: "integer", nullable: false),
                    OptionName = table.Column<string>(type: "text", nullable: false),
                    Multiplier = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculationOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalculationOption_CalculationParameter_CalculationParameter~",
                        column: x => x.CalculationParameterId,
                        principalTable: "CalculationParameter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CalculationParameter",
                columns: new[] { "Id", "DefaultValue", "Description", "Name", "ParameterType" },
                values: new object[,]
                {
                    { 1, 1.0m, "Highlights Difficulty ", "HighlightsDifficulty", "Option" },
                    { 2, 1.0m, "Pre-Parts Percentage", "PrePartsPercentage", "Option" },
                    { 3, 1.0m, "Resolution of video", "Resolution", "Option" },
                    { 4, 1.0m, "Quality of Footage", "FootageQuality", "Option" },
                    { 5, 0.3m, "Multiplier for camera when there exist more than 2", "CameraMulti", "Decimal" },
                    { 6, 0.4m, "Multiplier for raw footage size when bigger than 300gb", "SizeMulti", "Decimal" }
                });

            migrationBuilder.InsertData(
                table: "CalculationOption",
                columns: new[] { "Id", "CalculationParameterId", "Multiplier", "OptionName" },
                values: new object[,]
                {
                    { 1, 1, 0.9m, "Straight Forward Linear, little mixing" },
                    { 2, 1, 1m, "Hybrid Mostly Linear" },
                    { 3, 1, 1.2m, "Movie with heavy SFX + VFX" },
                    { 4, 2, 1m, "0%" },
                    { 5, 2, 0.95m, "30%" },
                    { 6, 2, 0.85m, "60%" },
                    { 7, 2, 0.7m, "100%" },
                    { 8, 3, 1m, "1080p" },
                    { 9, 3, 1.05m, "Mixed" },
                    { 10, 3, 1.1m, "4k" },
                    { 11, 4, 1.15m, "Needs work" },
                    { 12, 4, 1.05m, "Mostly good" },
                    { 13, 4, 1m, "Great" },
                    { 14, 4, 0.9m, "Excellent" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalculationOption_CalculationParameterId",
                table: "CalculationOption",
                column: "CalculationParameterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalculationOption");

            migrationBuilder.DropTable(
                name: "CalculationParameter");
        }
    }
}
