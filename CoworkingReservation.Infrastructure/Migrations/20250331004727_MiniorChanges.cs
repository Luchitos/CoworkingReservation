using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoworkingReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MiniorChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoworkingAreas_CoworkingSpaces_CoworkingSpaceId",
                table: "CoworkingAreas");

            migrationBuilder.AddForeignKey(
                name: "FK_CoworkingAreas_CoworkingSpaces_CoworkingSpaceId",
                table: "CoworkingAreas",
                column: "CoworkingSpaceId",
                principalTable: "CoworkingSpaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoworkingAreas_CoworkingSpaces_CoworkingSpaceId",
                table: "CoworkingAreas");

            migrationBuilder.AddForeignKey(
                name: "FK_CoworkingAreas_CoworkingSpaces_CoworkingSpaceId",
                table: "CoworkingAreas",
                column: "CoworkingSpaceId",
                principalTable: "CoworkingSpaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
