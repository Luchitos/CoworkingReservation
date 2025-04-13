using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoworkingReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExplicitCoworkingSpaceIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Verificar si la columna existe antes de crearla
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = N'CoworkingSpaceId'
                    AND Object_ID = Object_ID(N'CoworkingAreas')
                )
                BEGIN
                    ALTER TABLE CoworkingAreas 
                    ADD CoworkingSpaceId int NOT NULL DEFAULT 0;

                    ALTER TABLE CoworkingAreas 
                    ADD CONSTRAINT FK_CoworkingAreas_CoworkingSpaces_CoworkingSpaceId 
                    FOREIGN KEY (CoworkingSpaceId) REFERENCES CoworkingSpaces(Id) 
                    ON DELETE CASCADE;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No eliminamos la columna en Down para evitar pérdida de datos
        }
    }
}
