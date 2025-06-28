using CoworkingReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoworkingReservation.Tests.Helpers
{
    public static class DbContextHelper
    {
        public static ApplicationDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite("Filename=:memory:") // SQLite in-memory
                .Options;

            var context = new ApplicationDbContext(options);

            // Creamos la base de datos y sus tablas en memoria
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            return context;
        }

        public static void CleanDatabase(ApplicationDbContext context)
        {
            // Elimina todos los datos de las tablas sin dropear la base
            context.Reservations.RemoveRange(context.Reservations);
            context.CoworkingAreas.RemoveRange(context.CoworkingAreas);
            context.CoworkingSpaces.RemoveRange(context.CoworkingSpaces);
            context.Addresses.RemoveRange(context.Addresses);
            context.Users.RemoveRange(context.Users);

            // Agregá más tablas si las usás en tus tests

            context.SaveChanges();
        }
    }
}
