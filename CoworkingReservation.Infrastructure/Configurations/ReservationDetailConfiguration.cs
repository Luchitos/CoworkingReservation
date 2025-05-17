using CoworkingReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoworkingReservation.Infrastructure.Configurations
{
    public class ReservationDetailConfiguration : IEntityTypeConfiguration<ReservationDetail>
    {
        public void Configure(EntityTypeBuilder<ReservationDetail> builder)
        {
            builder.HasKey(rd => rd.Id);

            builder.Property(rd => rd.PricePerDay)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Relaciones
            builder.HasOne(rd => rd.Reservation)
                .WithMany(r => r.ReservationDetails)
                .HasForeignKey(rd => rd.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rd => rd.CoworkingArea)
                .WithMany()
                .HasForeignKey(rd => rd.CoworkingAreaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 