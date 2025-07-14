using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoworkingReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPhoto_Users_UserId",
                table: "UserPhoto");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserPhoto_PhotoId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPhoto",
                table: "UserPhoto");

            migrationBuilder.RenameTable(
                name: "UserPhoto",
                newName: "UserPhotos");

            migrationBuilder.RenameIndex(
                name: "IX_UserPhoto_UserId",
                table: "UserPhotos",
                newName: "IX_UserPhotos_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPhotos",
                table: "UserPhotos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPhotos_Users_UserId",
                table: "UserPhotos",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserPhotos_PhotoId",
                table: "Users",
                column: "PhotoId",
                principalTable: "UserPhotos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPhotos_Users_UserId",
                table: "UserPhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserPhotos_PhotoId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPhotos",
                table: "UserPhotos");

            migrationBuilder.RenameTable(
                name: "UserPhotos",
                newName: "UserPhoto");

            migrationBuilder.RenameIndex(
                name: "IX_UserPhotos_UserId",
                table: "UserPhoto",
                newName: "IX_UserPhoto_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPhoto",
                table: "UserPhoto",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPhoto_Users_UserId",
                table: "UserPhoto",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserPhoto_PhotoId",
                table: "Users",
                column: "PhotoId",
                principalTable: "UserPhoto",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
