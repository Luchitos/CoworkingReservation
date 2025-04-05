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
                        PasswordHash = "AQAAAAIAAYagAAAAEF0tiR+lvAIdSE6DyHk1oCbVinKEMAk+Jf1jEokRfMEmMVV7frZI0QWBfykCAwk0Eg==",
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
                        PasswordHash = "AQAAAAIAAYagAAAAEF0tiR+lvAIdSE6DyHk1oCbVinKEMAk+Jf1jEokRfMEmMVV7frZI0QWBfykCAwk0Eg==",
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
                        PasswordHash = "AQAAAAIAAYagAAAAEF0tiR+lvAIdSE6DyHk1oCbVinKEMAk+Jf1jEokRfMEmMVV7frZI0QWBfykCAwk0Eg==",
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
                        PasswordHash = "AQAAAAIAAYagAAAAEF0tiR+lvAIdSE6DyHk1oCbVinKEMAk+Jf1jEokRfMEmMVV7frZI0QWBfykCAwk0Eg==",
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
                        CapacityTotal = 20,
                        IsActive = true,
                        Address = addresses[0],
                        HosterId = users.First(u => u.UserName == "carlosl").Id
                    },
                    new CoworkingSpace
                    {
                        Name = "Coworking Space 2",
                        Description = "A cozy coworking space in Córdoba.",
                        CapacityTotal = 15,
                        IsActive = true,
                        Address = addresses[1],
                        HosterId = users.First(u => u.UserName == "carlosl").Id
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

                if (!await context.SafetyElements.AnyAsync())
                {
                    var safetyElements = new List<SafetyElement>
                    {
                        new() { Name = "Fire Extinguisher", Description = "Fire extinguishing device available in case of emergency." },
                        new() { Name = "First Aid Kit", Description = "Supplies available for minor injuries and emergencies." },
                        new() { Name = "Security Cameras", Description = "24/7 surveillance in common areas." },
                        new() { Name = "Smoke Detectors", Description = "Detects smoke and alerts in case of fire." },
                        new() { Name = "Emergency Exit Signs", Description = "Clearly marked emergency exit routes." },
                        new() { Name = "Sprinkler System", Description = "Automatic water sprinkler for fire control." },
                        new() { Name = "Security Guards", Description = "Personnel for on-site protection." },
                        new() { Name = "Access Control", Description = "Restricted access using badges or PINs." },
                        new() { Name = "Earthquake Resistant Structure", Description = "Building designed to withstand earthquakes." },
                        new() { Name = "Evacuation Plan", Description = "Posted instructions for safe exit." },
                        new() { Name = "CCTV Monitoring", Description = "Monitored cameras in hallways and entrances." },
                        new() { Name = "Fire Blankets", Description = "Used to smother small fires." },
                        new() { Name = "Automatic Doors", Description = "Opens automatically during fire alarms." },
                        new() { Name = "Fire Alarm System", Description = "Audible alert in case of smoke or fire." },
                        new() { Name = "Sanitizer Stations", Description = "Hand sanitizer at multiple entry points." },
                        new() { Name = "Non-slip Flooring", Description = "Reduces risk of slipping in wet areas." },
                        new() { Name = "Panic Buttons", Description = "Emergency alert system in each room." },
                        new() { Name = "Gas Leak Detectors", Description = "Alerts when gas leaks are detected." },
                        new() { Name = "Carbon Monoxide Detector", Description = "Senses and warns of CO presence." },
                        new() { Name = "Child Safety Locks", Description = "Increased safety in common areas." }
                    };

                    await context.SafetyElements.AddRangeAsync(safetyElements);
                    await context.SaveChangesAsync();
                }

                if (!await context.SpecialFeatures.AnyAsync())
                {
                    var specialFeatures = new List<SpecialFeature>
                        {
                            new() { Name = "24/7 Access", Description = "Access the coworking space anytime." },
                            new() { Name = "Private Office", Description = "Dedicated, enclosed space for privacy." },
                            new() { Name = "Virtual Office", Description = "Receive mail and calls remotely." },
                            new() { Name = "Free Parking", Description = "Parking spots available for members." },
                            new() { Name = "Event Hosting", Description = "Spaces available for events and workshops." },
                            new() { Name = "Pet-Friendly", Description = "Pets are welcome in the facility." },
                            new() { Name = "Gym Access", Description = "Fitness center available for members." },
                            new() { Name = "On-Site Café", Description = "Enjoy snacks and drinks without leaving." },
                            new() { Name = "Rooftop Lounge", Description = "Outdoor space to relax or work." },
                            new() { Name = "Nap Rooms", Description = "Quiet rooms available for short naps." },
                            new() { Name = "Library Room", Description = "Silent reading and research area." },
                            new() { Name = "Gaming Zone", Description = "Break space with recreational games." },
                            new() { Name = "Green Terrace", Description = "Garden space for breaks and meetings." },
                            new() { Name = "Massage Chairs", Description = "Relaxation spots with massage seating." },
                            new() { Name = "Bike Storage", Description = "Indoor bicycle parking." },
                            new() { Name = "Free Snacks", Description = "Complimentary snacks for users." },
                            new() { Name = "Meditation Room", Description = "Dedicated space for mindfulness." },
                            new() { Name = "Phone Booths", Description = "Private calling pods available." },
                            new() { Name = "Podcast Studio", Description = "Soundproof space for recordings." },
                            new() { Name = "Mail Handling", Description = "Staff will manage incoming mail." }
                        };

                    await context.SpecialFeatures.AddRangeAsync(specialFeatures);
                    await context.SaveChangesAsync();
                }


            }
        }
    }
}

