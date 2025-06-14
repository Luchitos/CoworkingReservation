﻿using CoworkingReservation.Domain.DTOs;
using CoworkingReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace CoworkingReservation.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSets
        public DbSet<Address> Addresses { get; set; }
        public DbSet<CoworkingSpace> CoworkingSpaces { get; set; }
        public DbSet<FavoriteCoworkingSpace> FavoriteCoworkingSpaces { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationDetail> ReservationDetails { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Benefit> Benefits { get; set; }
        public DbSet<ServiceOffered> ServicesOffered { get; set; }
        public DbSet<SpecialFeature> SpecialFeatures { get; set; }
        public DbSet<SafetyElement> SafetyElements { get; set; }
        public DbSet<CoworkingArea> CoworkingAreas { get; set; }
        public DbSet<CoworkingAvailability> CoworkingAvailabilities { get; set; }
        public DbSet<CoworkingSpacePhoto> CoworkingSpacePhotos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurar clave primaria compuesta para FavoriteCoworkingSpace
            modelBuilder.Entity<FavoriteCoworkingSpace>()
                .HasKey(fcs => new { fcs.UserId, fcs.CoworkingSpaceId });

            modelBuilder.Entity<FavoriteCoworkingSpace>()
                .HasOne(fcs => fcs.User)
                .WithMany(u => u.FavoriteCoworkingSpaces)
                .HasForeignKey(fcs => fcs.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FavoriteCoworkingSpace>()
                .HasOne(fcs => fcs.CoworkingSpace)
                .WithMany(cs => cs.FavoritedByUsers)
                .HasForeignKey(fcs => fcs.CoworkingSpaceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Photo)
                .WithMany() // Configuración unidireccional
                .HasForeignKey(u => u.PhotoId)
                .OnDelete(DeleteBehavior.SetNull); // Permitir fotos nulas sin eliminar usuario

            modelBuilder.Entity<UserPhoto>()
                .HasOne(up => up.User)
                .WithMany() // Cambia si necesitas navegación inversa
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Evitar eliminación en cascada

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

            modelBuilder.Entity<Review>()
    .HasOne(r => r.Reservation)
    .WithMany()
    .HasForeignKey(r => r.ReservationId)
    .OnDelete(DeleteBehavior.Restrict);
            // Configuración para evitar truncamientos en decimales

            modelBuilder.Entity<CoworkingSpace>()
                .HasOne(cs => cs.Hoster)
                .WithMany()
                .HasForeignKey(cs => cs.HosterId)
                .OnDelete(DeleteBehavior.Restrict); // Evita ON DELETE CASCADE

            modelBuilder.Entity<Reservation>()
                .Property(r => r.TotalPrice)
                .HasColumnType("decimal(18,2)");
            // **Relación Hoster - CoworkingSpace**
            modelBuilder.Entity<CoworkingSpace>()
                .HasOne(cs => cs.Hoster)
                .WithMany()
                .HasForeignKey(cs => cs.HosterId)
                .OnDelete(DeleteBehavior.Restrict);


            // ✅ **Nuevas Configuraciones para CoworkingArea y CoworkingAvailability**

            // **CoworkingArea -> CoworkingSpace**
            modelBuilder.Entity<CoworkingArea>()
                .HasOne(ca => ca.CoworkingSpace)
                .WithMany(cs => cs.Areas)
                .HasForeignKey(ca => ca.CoworkingSpaceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Especificar el tipo de columna para PricePerDay
            modelBuilder.Entity<CoworkingArea>()
                .Property(ca => ca.PricePerDay)
                .HasColumnType("decimal(18,2)");

            // **CoworkingAvailability -> CoworkingArea**
            modelBuilder.Entity<CoworkingAvailability>()
                .HasOne(ca => ca.CoworkingArea)
                .WithMany(area => area.Availabilities)
                .HasForeignKey(ca => ca.CoworkingAreaId)
                .OnDelete(DeleteBehavior.Cascade); // Si se elimina el área, se eliminan las disponibilidades

            modelBuilder.Entity<Reservation>()
                .Property(r => r.TotalPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<SpecialFeature>()
                .Property(sf => sf.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<SafetyElement>()
                .Property(se => se.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Configure many-to-many relationships for Benefit, SafetyElement, ServiceOffered, and SpecialFeature
            // These were previously defined by navigation properties that have been removed

            modelBuilder.Entity<CoworkingSpace>()
                .HasMany(cs => cs.Benefits)
                .WithMany()
                .UsingEntity(j => j.ToTable("BenefitCoworkingSpace"));

            modelBuilder.Entity<CoworkingSpace>()
                .HasMany(cs => cs.SafetyElements)
                .WithMany()
                .UsingEntity(j => j.ToTable("CoworkingSpaceSafetyElement"));

            modelBuilder.Entity<CoworkingSpace>()
                .HasMany(cs => cs.Services)
                .WithMany()
                .UsingEntity(j => j.ToTable("CoworkingSpaceServiceOffered"));

            modelBuilder.Entity<CoworkingSpace>()
                .HasMany(cs => cs.SpecialFeatures)
                .WithMany()
                .UsingEntity(j => j.ToTable("CoworkingSpaceSpecialFeature"));

            base.OnModelCreating(modelBuilder);
        }
    }
}
