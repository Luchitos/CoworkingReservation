using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoworkingReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addElementandSpecialService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SafetyElements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyElements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpecialFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialFeatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoworkingSpaceSafetyElement",
                columns: table => new
                {
                    CoworkingSpacesId = table.Column<int>(type: "int", nullable: false),
                    SafetyElementsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoworkingSpaceSafetyElement", x => new { x.CoworkingSpacesId, x.SafetyElementsId });
                    table.ForeignKey(
                        name: "FK_CoworkingSpaceSafetyElement_CoworkingSpaces_CoworkingSpacesId",
                        column: x => x.CoworkingSpacesId,
                        principalTable: "CoworkingSpaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoworkingSpaceSafetyElement_SafetyElements_SafetyElementsId",
                        column: x => x.SafetyElementsId,
                        principalTable: "SafetyElements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoworkingSpaceSpecialFeature",
                columns: table => new
                {
                    CoworkingSpacesId = table.Column<int>(type: "int", nullable: false),
                    SpecialFeaturesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoworkingSpaceSpecialFeature", x => new { x.CoworkingSpacesId, x.SpecialFeaturesId });
                    table.ForeignKey(
                        name: "FK_CoworkingSpaceSpecialFeature_CoworkingSpaces_CoworkingSpacesId",
                        column: x => x.CoworkingSpacesId,
                        principalTable: "CoworkingSpaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoworkingSpaceSpecialFeature_SpecialFeatures_SpecialFeaturesId",
                        column: x => x.SpecialFeaturesId,
                        principalTable: "SpecialFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoworkingSpaceSafetyElement_SafetyElementsId",
                table: "CoworkingSpaceSafetyElement",
                column: "SafetyElementsId");

            migrationBuilder.CreateIndex(
                name: "IX_CoworkingSpaceSpecialFeature_SpecialFeaturesId",
                table: "CoworkingSpaceSpecialFeature",
                column: "SpecialFeaturesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoworkingSpaceSafetyElement");

            migrationBuilder.DropTable(
                name: "CoworkingSpaceSpecialFeature");

            migrationBuilder.DropTable(
                name: "SafetyElements");

            migrationBuilder.DropTable(
                name: "SpecialFeatures");
        }
    }
}
