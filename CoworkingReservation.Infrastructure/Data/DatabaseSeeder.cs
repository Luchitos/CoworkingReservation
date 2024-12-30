using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Users.AnyAsync())
            {
                // **Usuarios**
                var users = new List<User>
                {
                    new User
                    {
                        Name = "Admin",
                        Lastname = "System",
                        UserName = "admin",
                        Cuit = "12345678901",
                        Email = "admin@coworking.com",
                        PasswordHash = "hashedpassword",
                        Role = "Admin",
                        IsActive = true
                    },
                    new User
                    {
                        Name = "Juan",
                        Lastname = "Pérez",
                        UserName = "juanp",
                        Cuit = "20345678901",
                        Email = "juanp@gmail.com",
                        PasswordHash = "hashedpassword",
                        Role = "Client",
                        IsActive = true
                    },
                    new User
                    {
                        Name = "María",
                        Lastname = "González",
                        UserName = "mariag",
                        Cuit = "27345678901",
                        Email = "mariag@gmail.com",
                        PasswordHash = "hashedpassword",
                        Role = "Client",
                        IsActive = true
                    },
                    new User
                    {
                        Name = "Carlos",
                        Lastname = "López",
                        UserName = "carlosl",
                        Cuit = "20345678902",
                        Email = "carlosl@gmail.com",
                        PasswordHash = "hashedpassword",
                        Role = "Hoster",
                        IsActive = true
                    }
                };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();

                // **Direcciones**
                var addresses = new List<Address>
                {
                    new Address
                    {
                        City = "Buenos Aires",
                        Country = "Argentina",
                        Street = "Calle Falsa",
                        Number = "123",
                        Province = "Buenos Aires",
                        ZipCode = "1000"
                    },
                    new Address
                    {
                        City = "Córdoba",
                        Country = "Argentina",
                        Street = "Avenida Siempre Viva",
                        Number = "742",
                        Province = "Córdoba",
                        ZipCode = "5000"
                    }
                };

                await context.Addresses.AddRangeAsync(addresses);
                await context.SaveChangesAsync();

                // **Espacios de Coworking**
                var coworkingSpaces = new List<CoworkingSpace>
                {
                    new CoworkingSpace
                    {
                        Name = "Coworking Space 1",
                        Description = "A modern coworking space in Buenos Aires.",
                        Capacity = 20,
                        PricePerDay = 1500,
                        AddressId = addresses[0].Id,
                        IsActive = true
                    },
                    new CoworkingSpace
                    {
                        Name = "Coworking Space 2",
                        Description = "A cozy coworking space in Córdoba.",
                        Capacity = 15,
                        PricePerDay = 1200,
                        AddressId = addresses[1].Id,
                        IsActive = true
                    }
                };

                await context.CoworkingSpaces.AddRangeAsync(coworkingSpaces);
                await context.SaveChangesAsync();

                // **Reservas**
                var reservations = new List<Reservation>
                {
                    new Reservation
                    {
                        UserId = users.First(u => u.UserName == "juanp").Id,
                        CoworkingSpaceId = coworkingSpaces[0].Id,
                        StartDate = DateTime.UtcNow.AddDays(1),
                        EndDate = DateTime.UtcNow.AddDays(3),
                        TotalPrice = 3000,
                        Status = ReservationStatus.Confirmed
                    },
                    new Reservation
                    {
                        UserId = users.First(u => u.UserName == "mariag").Id,
                        CoworkingSpaceId = coworkingSpaces[1].Id,
                        StartDate = DateTime.UtcNow.AddDays(2),
                        EndDate = DateTime.UtcNow.AddDays(4),
                        TotalPrice = 2400,
                        Status = ReservationStatus.Pending
                    }
                };

                await context.Reservations.AddRangeAsync(reservations);
                await context.SaveChangesAsync();

                // **Favoritos**
                var favorites = new List<FavoriteCoworkingSpace>
                {
                    new FavoriteCoworkingSpace
                    {
                        UserId = users.First(u => u.UserName == "juanp").Id,
                        CoworkingSpaceId = coworkingSpaces[0].Id
                    },
                    new FavoriteCoworkingSpace
                    {
                        UserId = users.First(u => u.UserName == "mariag").Id,
                        CoworkingSpaceId = coworkingSpaces[1].Id
                    }
                };

                await context.FavoriteCoworkingSpaces.AddRangeAsync(favorites);
                await context.SaveChangesAsync();
            }
        }
    }
}

