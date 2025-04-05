using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoworkingReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateCoworkingSpace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricePerDay",
                table: "CoworkingSpaces");

            migrationBuilder.RenameColumn(
                name: "Capacity",
                table: "CoworkingSpaces",
                newName: "CapacityTotal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CapacityTotal",
                table: "CoworkingSpaces",
                newName: "Capacity");

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerDay",
                table: "CoworkingSpaces",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
