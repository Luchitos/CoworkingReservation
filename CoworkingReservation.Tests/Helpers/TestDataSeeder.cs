using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Infrastructure.Data;

namespace CoworkingReservation.Tests.Helpers
{
    public static class TestDataSeeder
    {
        public static async Task SeedFullSet(ApplicationDbContext context)
        {
            for (int i = 1; i <= 50; i++)
            {
                var user = await SeedUser(context, $"user{i}", $"user{i}@mail.com");
                var address = await SeedAddress(context, i);
                var coworking = await SeedCoworkingSpace(context, user, address, i);
                var area = await SeedCoworkingArea(context, coworking, i);
                await SeedReservation(context, user, coworking, area, i);
            }
        }

        public static async Task<User> SeedUser(ApplicationDbContext context, string userName, string email)
        {
            var user = new User
            {
                Name = $"Name_{userName}",
                Lastname = $"Lastname_{userName}",
                UserName = userName,
                Cuit = $"20-12345678-9",
                Email = email,
                PasswordHash = "hashedpassword",
                Role = "Client"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        public static async Task<Address> SeedAddress(ApplicationDbContext context, int index = 1)
        {
            var address = new Address
            {
                Street = $"Test Street {index}",
                StreetOne = $"Cross Street {index}A",
                StreetTwo = $"Cross Street {index}B",
                Number = $"{100 + index}",
                City = $"City{index}",
                Province = $"Province{index}",
                Country = $"Country{index}",
                ZipCode = $"100{index}",
                Latitude = $"-34.60{index:D2}",
                Longitude = $"-58.38{index:D2}"
            };

            context.Addresses.Add(address);
            await context.SaveChangesAsync();
            return address;
        }

        public static async Task<CoworkingSpace> SeedCoworkingSpace(ApplicationDbContext context, User hoster, Address address, int index = 1)
        {
            var coworkingSpace = new CoworkingSpace
            {
                Name = $"Coworking Space {hoster.UserName}_{index}",
                Description = $"Description for coworking {hoster.UserName}_{index}",
                CapacityTotal = 50,
                Rate = 4.0f,
                Status = CoworkingStatus.Approved,
                HosterId = hoster.Id,
                Address = address
            };

            context.CoworkingSpaces.Add(coworkingSpace);
            await context.SaveChangesAsync();
            return coworkingSpace;
        }

        public static async Task<CoworkingArea> SeedCoworkingArea(ApplicationDbContext context, CoworkingSpace coworkingSpace, int index = 1)
        {
            var area = new CoworkingArea
            {
                CoworkingSpaceId = coworkingSpace.Id,
                Type = (CoworkingAreaType)(index % Enum.GetValues(typeof(CoworkingAreaType)).Length),
                Capacity = 5 + (index % 15),
                PricePerDay = 80 + (index * 2),
                Available = true
            };

            context.CoworkingAreas.Add(area);
            await context.SaveChangesAsync();
            return area;
        }

        public static async Task<Reservation> SeedReservation(ApplicationDbContext context, User user, CoworkingSpace coworkingSpace, CoworkingArea area, int index = 1)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(index);
            var endDate = startDate.AddDays(2);

            var reservation = new Reservation
            {
                UserId = user.Id,
                CoworkingSpaceId = coworkingSpace.Id,
                StartDate = startDate,
                EndDate = endDate,
                Status = ReservationStatus.Pending,
                TotalPrice = area.PricePerDay * 2,
                PaymentMethod = PaymentMethod.CreditCard,
                CreatedAt = DateTime.UtcNow,
                ReservationDetails = new List<ReservationDetail>
                {
                    new ReservationDetail
                    {
                        CoworkingAreaId = area.Id,
                        PricePerDay = area.PricePerDay
                    }
                }
            };

            context.Reservations.Add(reservation);
            await context.SaveChangesAsync();
            return reservation;
        }
    }
}
