using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoworkingReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCoworkingAreaRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Asegurarse de que hay un índice en la columna CoworkingSpaceId
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes 
                    WHERE name='IX_CoworkingAreas_CoworkingSpaceId' 
                    AND object_id = OBJECT_ID('CoworkingAreas')
                )
                BEGIN
                    CREATE INDEX [IX_CoworkingAreas_CoworkingSpaceId] ON [CoworkingAreas] ([CoworkingSpaceId]);
                END
            ");
            
            // Asegurarse de que la relación está configurada correctamente 
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys 
                    WHERE name='FK_CoworkingAreas_CoworkingSpaces_CoworkingSpaceId' 
                    AND parent_object_id = OBJECT_ID('CoworkingAreas')
                )
                BEGIN
                    ALTER TABLE CoworkingAreas 
                    DROP CONSTRAINT FK_CoworkingAreas_CoworkingSpaces_CoworkingSpaceId;
                END

                ALTER TABLE CoworkingAreas 
                ADD CONSTRAINT FK_CoworkingAreas_CoworkingSpaces_CoworkingSpaceId 
                FOREIGN KEY (CoworkingSpaceId) REFERENCES CoworkingSpaces(Id) 
                ON DELETE CASCADE;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No hacer nada aquí para evitar problemas
        }
    }
}
