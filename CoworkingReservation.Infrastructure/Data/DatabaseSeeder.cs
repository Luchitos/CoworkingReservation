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
                        IsActive = true,
                        Address = addresses[0], // 👈 Asignamos la dirección
                        HosterId = users.First(u => u.UserName == "carlosl").Id // 👈 Asignamos un hoster
                    },
                    new CoworkingSpace
                    {
                        Name = "Coworking Space 2",
                        Description = "A cozy coworking space in Córdoba.",
                        Capacity = 15,
                        PricePerDay = 1200,
                        IsActive = true,
                        Address = addresses[1], // 👈 Asignamos la dirección
                        HosterId = users.First(u => u.UserName == "carlosl").Id // 👈 Asignamos un hoster
                    }
                };

                if (!await context.ServicesOffered.AnyAsync())
                {
                    var services = new List<ServiceOffered>
                {
                    new ServiceOffered { Name = "Internet de alta velocidad", Description = "Conexión estable y rápida para todos los usuarios." },
                    new ServiceOffered { Name = "Oficinas privadas", Description = "Espacios cerrados para mayor privacidad." },
                    new ServiceOffered { Name = "Salas de reuniones", Description = "Salas equipadas con tecnología para conferencias." },
                    new ServiceOffered { Name = "Escritorios compartidos", Description = "Zonas de trabajo abiertas para networking." },
                    new ServiceOffered { Name = "Cafetería y bebidas", Description = "Café, té y otras bebidas incluidas." },
                    new ServiceOffered { Name = "Acceso 24/7", Description = "Disponibilidad total para trabajar en cualquier horario." },
                    new ServiceOffered { Name = "Impresoras y escáneres", Description = "Acceso a impresión y escaneo de documentos." },
                    new ServiceOffered { Name = "Gestión de correo", Description = "Recepción y manejo de correspondencia." },
                    new ServiceOffered { Name = "Eventos y talleres", Description = "Capacitaciones y networking en el espacio." },
                    new ServiceOffered { Name = "Espacios de descanso", Description = "Áreas de relajación para mayor comodidad." }
                };

                    await context.ServicesOffered.AddRangeAsync(services);
                    await context.SaveChangesAsync();
                }

                if (!await context.Benefits.AnyAsync())
                {
                    var benefits = new List<Benefit>
                {
                    new Benefit { Name = "Descuentos en servicios profesionales", Description = "Acceso a tarifas preferenciales en asesorías y consultorías." },
                    new Benefit { Name = "Acceso a gimnasios/actividades deportivas", Description = "Convenios con gimnasios y clubes deportivos." },
                    new Benefit { Name = "Descuentos en software y herramientas", Description = "Ofertas especiales en plataformas digitales." },
                    new Benefit { Name = "Descuento en transporte público/privado", Description = "Tarifas reducidas en movilidad urbana." },
                    new Benefit { Name = "Acceso a salas de eventos", Description = "Espacios exclusivos para reuniones y presentaciones." },
                    new Benefit { Name = "Descuentos en cafeterías/restaurantes", Description = "Ofertas en establecimientos cercanos." },
                    new Benefit { Name = "Cursos y formación con descuento", Description = "Acceso a capacitaciones a menor costo." },
                    new Benefit { Name = "Acceso a redes de inversionistas", Description = "Conexión con potenciales inversores y financiamiento." },
                    new Benefit { Name = "Descuento en eventos de networking", Description = "Participación en encuentros de negocios a precio especial." },
                    new Benefit { Name = "Alquiler gratuito de equipos/salas", Description = "Uso de recursos sin costo adicional." }
                };

                    await context.Benefits.AddRangeAsync(benefits);
                    await context.SaveChangesAsync();
                }

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

