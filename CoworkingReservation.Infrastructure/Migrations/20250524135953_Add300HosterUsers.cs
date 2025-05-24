using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoworkingReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add300HosterUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enable IDENTITY_INSERT for Users table to insert explicit IDs
            migrationBuilder.Sql("SET IDENTITY_INSERT Users ON");

            // Arrays with realistic Spanish names for generating users
            var firstNames = new[]
            {
                "Carlos", "María", "José", "Ana", "Luis", "Elena", "Miguel", "Laura", "Roberto", "Carmen",
                "Francisco", "Isabel", "Antonio", "Pilar", "Manuel", "Rosa", "Jesús", "Dolores", "Javier", "Mercedes",
                "Ángel", "Josefa", "Rafael", "Antonia", "Daniel", "Francisca", "Diego", "Teresa", "Pedro", "Concepción",
                "Alejandro", "Manuela", "Fernando", "Rosario", "Sergio", "Amparo", "Pablo", "Encarnación", "Jorge", "Soledad",
                "Alberto", "Remedios", "Adrián", "Gloria", "Álvaro", "Esperanza", "Óscar", "Purificación", "Rubén", "Inmaculada",
                "Raúl", "Milagros", "Enrique", "Guadalupe", "Ramón", "Asunción", "Vicente", "Montserrat", "Andrés", "Fuensanta",
                "Emilio", "Catalina", "Juan", "Lucía", "Joaquín", "Victoria", "Ignacio", "Margarita", "Santiago", "Cristina",
                "Tomás", "Raquel", "Gonzalo", "Silvia", "Nicolás", "Patricia", "Salvador", "Beatriz", "Agustín", "Mónica",
                "Marcos", "Nuria", "Lorenzo", "Susana", "Iván", "Yolanda", "Guillermo", "Verónica", "César", "Julia"
            };

            var lastNames = new[]
            {
                "García", "González", "López", "Martínez", "Sánchez", "Pérez", "Gómez", "Martín", "Jiménez", "Ruiz",
                "Hernández", "Díaz", "Moreno", "Muñoz", "Álvarez", "Romero", "Alonso", "Gutiérrez", "Navarro", "Torres",
                "Domínguez", "Vázquez", "Ramos", "Gil", "Ramírez", "Serrano", "Blanco", "Molina", "Morales", "Suárez",
                "Ortega", "Delgado", "Castro", "Ortiz", "Rubio", "Marín", "Sanz", "Iglesias", "Medina", "Garrido",
                "Cortés", "Castillo", "Santos", "Lozano", "Guerrero", "Cano", "Prieto", "Méndez", "Cruz", "Herrera",
                "Peña", "Flores", "Cabrera", "Campos", "Vega", "Fuentes", "Carrasco", "Diez", "Caballero", "Nieto",
                "Reyes", "Aguilar", "Pascual", "Herrero", "Montero", "Lorenzo", "Hidalgo", "Giménez", "Ibáñez", "Ferrer",
                "Duran", "Santiago", "Benítez", "Mora", "Vicente", "Arias", "Vargas", "Carmona", "Crespo", "Román",
                "Pastor", "Soto", "Sainz", "Villanueva", "Marcos", "Mendoza", "Calvo", "Fernández", "Espinosa", "Ribas"
            };

            // Password hash for "Password123!" - this should be a consistent hash for all users
            var passwordHash = "AQAAAAIAAYagAAAAEF0tiR+lvAIdSE6DyHk1oCbVinKEMAk+Jf1jEokRfMEmMVV7frZI0QWBfykCAwk0Eg==";
            var creationDate = DateTime.UtcNow;
            var random = new Random(42); // Fixed seed for reproducible results

            // Generate and insert 300 users
            for (int i = 1; i <= 300; i++)
            {
                var firstName = firstNames[random.Next(firstNames.Length)];
                var lastName = lastNames[random.Next(lastNames.Length)];
                var userName = $"{firstName.ToLower()}.{lastName.ToLower()}.{i}";
                var email = $"{firstName.ToLower()}.{lastName.ToLower()}.{i}@coworking.com";
                var cuit = $"20-{30000000 + i:D8}-{random.Next(1, 10)}";
                var userId = i; // IDs from 1 to 300

                var sql = $@"
                    INSERT INTO Users (Id, Name, Lastname, UserName, Cuit, Email, PasswordHash, Role, IsActive, CreationDate)
                    VALUES ({userId}, '{firstName}', '{lastName}', '{userName}', '{cuit}', '{email}', '{passwordHash}', 'Hoster', 1, '{creationDate:yyyy-MM-dd HH:mm:ss}');";

                migrationBuilder.Sql(sql);
            }

            // Disable IDENTITY_INSERT for Users table
            migrationBuilder.Sql("SET IDENTITY_INSERT Users OFF");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete the 300 Hoster users (IDs 1-300)
            migrationBuilder.Sql("DELETE FROM Users WHERE Id BETWEEN 1 AND 300 AND Role = 'Hoster'");
        }
    }
}
