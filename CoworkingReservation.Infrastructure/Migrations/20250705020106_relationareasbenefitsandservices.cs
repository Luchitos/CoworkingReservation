using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoworkingReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class relationareasbenefitsandservices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BenefitCoworkingSpace_Benefits_BenefitsId",
                table: "BenefitCoworkingSpace");

            migrationBuilder.DropForeignKey(
                name: "FK_CoworkingSpaceServiceOffered_ServicesOffered_ServicesId",
                table: "CoworkingSpaceServiceOffered");

            migrationBuilder.RenameColumn(
                name: "ServicesId",
                table: "CoworkingSpaceServiceOffered",
                newName: "ServiceOfferedId");

            migrationBuilder.RenameIndex(
                name: "IX_CoworkingSpaceServiceOffered_ServicesId",
                table: "CoworkingSpaceServiceOffered",
                newName: "IX_CoworkingSpaceServiceOffered_ServiceOfferedId");

            migrationBuilder.RenameColumn(
                name: "BenefitsId",
                table: "BenefitCoworkingSpace",
                newName: "BenefitId");

            migrationBuilder.AddForeignKey(
                name: "FK_BenefitCoworkingSpace_Benefits_BenefitId",
                table: "BenefitCoworkingSpace",
                column: "BenefitId",
                principalTable: "Benefits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoworkingSpaceServiceOffered_ServicesOffered_ServiceOfferedId",
                table: "CoworkingSpaceServiceOffered",
                column: "ServiceOfferedId",
                principalTable: "ServicesOffered",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BenefitCoworkingSpace_Benefits_BenefitId",
                table: "BenefitCoworkingSpace");

            migrationBuilder.DropForeignKey(
                name: "FK_CoworkingSpaceServiceOffered_ServicesOffered_ServiceOfferedId",
                table: "CoworkingSpaceServiceOffered");

            migrationBuilder.RenameColumn(
                name: "ServiceOfferedId",
                table: "CoworkingSpaceServiceOffered",
                newName: "ServicesId");

            migrationBuilder.RenameIndex(
                name: "IX_CoworkingSpaceServiceOffered_ServiceOfferedId",
                table: "CoworkingSpaceServiceOffered",
                newName: "IX_CoworkingSpaceServiceOffered_ServicesId");

            migrationBuilder.RenameColumn(
                name: "BenefitId",
                table: "BenefitCoworkingSpace",
                newName: "BenefitsId");

            migrationBuilder.AddForeignKey(
                name: "FK_BenefitCoworkingSpace_Benefits_BenefitsId",
                table: "BenefitCoworkingSpace",
                column: "BenefitsId",
                principalTable: "Benefits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoworkingSpaceServiceOffered_ServicesOffered_ServicesId",
                table: "CoworkingSpaceServiceOffered",
                column: "ServicesId",
                principalTable: "ServicesOffered",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
