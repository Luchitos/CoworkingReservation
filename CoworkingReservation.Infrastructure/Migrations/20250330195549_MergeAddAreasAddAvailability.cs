using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoworkingReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MergeAddAreasAddAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoworkingAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    PricePerDay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoworkingSpaceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoworkingAreas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoworkingAreas_CoworkingSpaces_CoworkingSpaceId",
                        column: x => x.CoworkingSpaceId,
                        principalTable: "CoworkingSpaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoworkingAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AvailableSpots = table.Column<int>(type: "int", nullable: false),
                    CoworkingAreaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoworkingAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoworkingAvailabilities_CoworkingAreas_CoworkingAreaId",
                        column: x => x.CoworkingAreaId,
                        principalTable: "CoworkingAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoworkingAreas_CoworkingSpaceId",
                table: "CoworkingAreas",
                column: "CoworkingSpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_CoworkingAvailabilities_CoworkingAreaId",
                table: "CoworkingAvailabilities",
                column: "CoworkingAreaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoworkingAvailabilities");

            migrationBuilder.DropTable(
                name: "CoworkingAreas");
        }
    }
}
