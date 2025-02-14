using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoworkingReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBenefitsToDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Benefits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Benefits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicesOffered",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicesOffered", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BenefitCoworkingSpace",
                columns: table => new
                {
                    BenefitsId = table.Column<int>(type: "int", nullable: false),
                    CoworkingSpacesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenefitCoworkingSpace", x => new { x.BenefitsId, x.CoworkingSpacesId });
                    table.ForeignKey(
                        name: "FK_BenefitCoworkingSpace_Benefits_BenefitsId",
                        column: x => x.BenefitsId,
                        principalTable: "Benefits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BenefitCoworkingSpace_CoworkingSpaces_CoworkingSpacesId",
                        column: x => x.CoworkingSpacesId,
                        principalTable: "CoworkingSpaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoworkingSpaceServiceOffered",
                columns: table => new
                {
                    CoworkingSpacesId = table.Column<int>(type: "int", nullable: false),
                    ServicesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoworkingSpaceServiceOffered", x => new { x.CoworkingSpacesId, x.ServicesId });
                    table.ForeignKey(
                        name: "FK_CoworkingSpaceServiceOffered_CoworkingSpaces_CoworkingSpacesId",
                        column: x => x.CoworkingSpacesId,
                        principalTable: "CoworkingSpaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoworkingSpaceServiceOffered_ServicesOffered_ServicesId",
                        column: x => x.ServicesId,
                        principalTable: "ServicesOffered",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BenefitCoworkingSpace_CoworkingSpacesId",
                table: "BenefitCoworkingSpace",
                column: "CoworkingSpacesId");

            migrationBuilder.CreateIndex(
                name: "IX_CoworkingSpaceServiceOffered_ServicesId",
                table: "CoworkingSpaceServiceOffered",
                column: "ServicesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BenefitCoworkingSpace");

            migrationBuilder.DropTable(
                name: "CoworkingSpaceServiceOffered");

            migrationBuilder.DropTable(
                name: "Benefits");

            migrationBuilder.DropTable(
                name: "ServicesOffered");
        }
    }
}
