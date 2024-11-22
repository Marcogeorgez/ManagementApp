using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class EditorPaymentAndRemoveOld : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientPayment");

            migrationBuilder.RenameColumn(
                name: "Billable",
                table: "Projects",
                newName: "ClientBillable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClientBillable",
                table: "Projects",
                newName: "Billable");

            migrationBuilder.CreateTable(
                name: "ClientPayment",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "character varying(255)", nullable: true),
                    BillableHours = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ClientPaymentStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PaymentVisibleClient = table.Column<bool>(type: "boolean", nullable: false),
                    ProjectTotalPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientPayment", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_ClientPayment_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientPayment_Users_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientPayment_ApplicationUserId",
                table: "ClientPayment",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientPayment_ProjectId",
                table: "ClientPayment",
                column: "ProjectId",
                unique: true);
        }
    }
}
