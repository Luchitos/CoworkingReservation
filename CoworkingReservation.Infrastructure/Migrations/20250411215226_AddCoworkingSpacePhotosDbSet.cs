using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoworkingReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoworkingSpacePhotosDbSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoworkingSpacePhoto_CoworkingSpaces_CoworkingSpaceId",
                table: "CoworkingSpacePhoto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoworkingSpacePhoto",
                table: "CoworkingSpacePhoto");

            migrationBuilder.RenameTable(
                name: "CoworkingSpacePhoto",
                newName: "CoworkingSpacePhotos");

            migrationBuilder.RenameIndex(
                name: "IX_CoworkingSpacePhoto_CoworkingSpaceId",
                table: "CoworkingSpacePhotos",
                newName: "IX_CoworkingSpacePhotos_CoworkingSpaceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoworkingSpacePhotos",
                table: "CoworkingSpacePhotos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CoworkingSpacePhotos_CoworkingSpaces_CoworkingSpaceId",
                table: "CoworkingSpacePhotos",
                column: "CoworkingSpaceId",
                principalTable: "CoworkingSpaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoworkingSpacePhotos_CoworkingSpaces_CoworkingSpaceId",
                table: "CoworkingSpacePhotos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoworkingSpacePhotos",
                table: "CoworkingSpacePhotos");

            migrationBuilder.RenameTable(
                name: "CoworkingSpacePhotos",
                newName: "CoworkingSpacePhoto");

            migrationBuilder.RenameIndex(
                name: "IX_CoworkingSpacePhotos_CoworkingSpaceId",
                table: "CoworkingSpacePhoto",
                newName: "IX_CoworkingSpacePhoto_CoworkingSpaceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoworkingSpacePhoto",
                table: "CoworkingSpacePhoto",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CoworkingSpacePhoto_CoworkingSpaces_CoworkingSpaceId",
                table: "CoworkingSpacePhoto",
                column: "CoworkingSpaceId",
                principalTable: "CoworkingSpaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
