using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoworkingReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoriteCoworkingSpaces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteCoworkingSpace_CoworkingSpaces_CoworkingSpaceId",
                table: "FavoriteCoworkingSpace");

            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteCoworkingSpace_Users_UserId",
                table: "FavoriteCoworkingSpace");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoriteCoworkingSpace",
                table: "FavoriteCoworkingSpace");

            migrationBuilder.RenameTable(
                name: "FavoriteCoworkingSpace",
                newName: "FavoriteCoworkingSpaces");

            migrationBuilder.RenameIndex(
                name: "IX_FavoriteCoworkingSpace_CoworkingSpaceId",
                table: "FavoriteCoworkingSpaces",
                newName: "IX_FavoriteCoworkingSpaces_CoworkingSpaceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavoriteCoworkingSpaces",
                table: "FavoriteCoworkingSpaces",
                columns: new[] { "UserId", "CoworkingSpaceId" });

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteCoworkingSpaces_CoworkingSpaces_CoworkingSpaceId",
                table: "FavoriteCoworkingSpaces",
                column: "CoworkingSpaceId",
                principalTable: "CoworkingSpaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteCoworkingSpaces_Users_UserId",
                table: "FavoriteCoworkingSpaces",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteCoworkingSpaces_CoworkingSpaces_CoworkingSpaceId",
                table: "FavoriteCoworkingSpaces");

            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteCoworkingSpaces_Users_UserId",
                table: "FavoriteCoworkingSpaces");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoriteCoworkingSpaces",
                table: "FavoriteCoworkingSpaces");

            migrationBuilder.RenameTable(
                name: "FavoriteCoworkingSpaces",
                newName: "FavoriteCoworkingSpace");

            migrationBuilder.RenameIndex(
                name: "IX_FavoriteCoworkingSpaces_CoworkingSpaceId",
                table: "FavoriteCoworkingSpace",
                newName: "IX_FavoriteCoworkingSpace_CoworkingSpaceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavoriteCoworkingSpace",
                table: "FavoriteCoworkingSpace",
                columns: new[] { "UserId", "CoworkingSpaceId" });

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteCoworkingSpace_CoworkingSpaces_CoworkingSpaceId",
                table: "FavoriteCoworkingSpace",
                column: "CoworkingSpaceId",
                principalTable: "CoworkingSpaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteCoworkingSpace_Users_UserId",
                table: "FavoriteCoworkingSpace",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
