using CoworkingReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace CoworkingReservation.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<CoworkingSpace> CoworkingSpaces { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de relaciones y restricciones
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Evitar cascada en eliminación

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.CoworkingSpace)
                .WithMany(cs => cs.Reservations)
                .HasForeignKey(r => r.CoworkingSpaceId)
                .OnDelete(DeleteBehavior.Restrict); // Evitar cascada en eliminación

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.CoworkingSpace)
                .WithMany(cs => cs.Reviews)
                .HasForeignKey(r => r.CoworkingSpaceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración para evitar truncamientos en decimales
            modelBuilder.Entity<CoworkingSpace>()
                .Property(cs => cs.PricePerDay)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Reservation>()
                .Property(r => r.TotalPrice)
                .HasColumnType("decimal(18,2)");

            base.OnModelCreating(modelBuilder);
        }
    }
}
