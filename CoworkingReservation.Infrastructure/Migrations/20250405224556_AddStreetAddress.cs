using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoworkingReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStreetAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StreetOne",
                table: "Addresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StreetTwo",
                table: "Addresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StreetOne",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "StreetTwo",
                table: "Addresses");
        }
    }
}
