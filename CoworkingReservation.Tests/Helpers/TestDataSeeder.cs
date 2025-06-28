using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using CoworkingReservation.Infrastructure.Data;


namespace CoworkingReservation.Tests.Helpers
{
    public static class TestDataSeeder
    {
        public static async Task<User> SeedUser(ApplicationDbContext context)
        {
            var user = new User
            {
                Id = 1,
                Name = "Test",
                Lastname = "User",
                UserName = "testuser",
                Cuit = "20-12345678-9",
                Email = "test@user.com",
                PasswordHash = "hashedpassword",
                Role = "Client"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        public static async Task<Address> SeedAddress(ApplicationDbContext context)
        {
            var address = new Address
            {
                Id = 1,
                Street = "Test Street",
                StreetOne = "Cross Street 1",
                StreetTwo = "Cross Street 2",
                Number = "123",
                City = "Test City",
                Province = "Test Province",
                Country = "Test Country",
                ZipCode = "1234",
                Latitude = "-34.6037",
                Longitude = "-58.3816"
            };

            context.Addresses.Add(address);
            await context.SaveChangesAsync();

            return address;
        }

        public static async Task<CoworkingSpace> SeedCoworkingSpace(ApplicationDbContext context, User hoster)
        {
            var address = await SeedAddress(context);

            var coworkingSpace = new CoworkingSpace
            {
                Id = 1,
                Name = "Test Coworking Space",
                Description = "A test coworking space",
                CapacityTotal = 100,
                Rate = 4.5f,
                Status = CoworkingStatus.Approved,
                HosterId = hoster.Id,
                Address = address
            };

            context.CoworkingSpaces.Add(coworkingSpace);
            await context.SaveChangesAsync();

            return coworkingSpace;
        }

        public static async Task<CoworkingArea> SeedCoworkingArea(ApplicationDbContext context, CoworkingSpace coworkingSpace)
        {
            var area = new CoworkingArea
            {
                Id = 1,
                CoworkingSpaceId = coworkingSpace.Id,
                Type = CoworkingAreaType.SharedDesks,
                Capacity = 10,
                PricePerDay = 100,
                Available = true
            };

            context.CoworkingAreas.Add(area);
            await context.SaveChangesAsync();

            return area;
        }

        public static async Task<Reservation> SeedReservation(ApplicationDbContext context, User user, CoworkingSpace coworkingSpace, CoworkingArea area)
        {
            var reservation = new Reservation
            {
                Id = 1,
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(3),
                Status = ReservationStatus.Pending,
                TotalPrice = 100 * 3,
                PaymentMethod = PaymentMethod.CreditCard,
                CreatedAt = DateTime.UtcNow,
                ReservationDetails = new List<ReservationDetail>
                {
                    new ReservationDetail
                    {
                        CoworkingAreaId = area.Id,
                        PricePerDay = 100
                    }
                }
            };

            context.Reservations.Add(reservation);
            await context.SaveChangesAsync();

            return reservation;
        }
    }
}
