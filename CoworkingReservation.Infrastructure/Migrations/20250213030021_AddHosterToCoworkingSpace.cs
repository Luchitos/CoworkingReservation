using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoworkingReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHosterToCoworkingSpace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoworkingSpaces_Users_UserId",
                table: "CoworkingSpaces");

            migrationBuilder.DropIndex(
                name: "IX_CoworkingSpaces_UserId",
                table: "CoworkingSpaces");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CoworkingSpaces");

            migrationBuilder.AddColumn<int>(
                name: "HosterId",
                table: "CoworkingSpaces",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CoworkingSpaces_HosterId",
                table: "CoworkingSpaces",
                column: "HosterId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoworkingSpaces_Users_HosterId",
                table: "CoworkingSpaces",
                column: "HosterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoworkingSpaces_Users_HosterId",
                table: "CoworkingSpaces");

            migrationBuilder.DropIndex(
                name: "IX_CoworkingSpaces_HosterId",
                table: "CoworkingSpaces");

            migrationBuilder.DropColumn(
                name: "HosterId",
                table: "CoworkingSpaces");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "CoworkingSpaces",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoworkingSpaces_UserId",
                table: "CoworkingSpaces",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoworkingSpaces_Users_UserId",
                table: "CoworkingSpaces",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
