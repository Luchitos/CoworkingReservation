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

            // Sembrar los 300 CoworkingSpaces básicos después de que todos los datos de catálogo estén listos
            await CoworkingSpaceSeeder.SeedCoworkingSpacesAsync(context);

            // Sembrar las relaciones BenefitCoworkingSpace
            await BenefitCoworkingSpaceSeeder.SeedBenefitCoworkingSpaceAsync(context);

            // Sembrar las relaciones CoworkingSpaceSafetyElement
            await CoworkingSpaceSafetyElementSeeder.SeedCoworkingSpaceSafetyElementAsync(context);

            // Sembrar las relaciones CoworkingSpaceServiceOffered
            await CoworkingSpaceServiceOfferedSeeder.SeedCoworkingSpaceServiceOfferedAsync(context);

            // Sembrar las relaciones CoworkingSpaceSpecialFeature
            await CoworkingSpaceSpecialFeatureSeeder.SeedCoworkingSpaceSpecialFeatureAsync(context);

            // Sembrar las fotos de los CoworkingSpaces
            await CoworkingSpacePhotosSeeder.SeedCoworkingSpacePhotosAsync(context);

            // Sembrar las áreas de los CoworkingSpaces
            await CoworkingAreaSeeder.SeedCoworkingAreasAsync(context);

            // Sembrar las disponibilidades de las áreas (debe ejecutarse antes de las reservas)
            await CoworkingAvailabilitySeeder.SeedCoworkingAvailabilitiesAsync(context);

            // Sembrar las reservas (pasadas, actuales y futuras)
            await ReservationSeeder.SeedReservationsAsync(context);

            // Sembrar las reviews basadas en reservas completadas
            await ReviewSeeder.SeedReviewsAsync(context);

            Console.WriteLine("🎉 ¡Seeding completo! Todas las relaciones many-to-many, fotos, áreas, disponibilidades, reservas y reviews han sido creadas exitosamente:");
            Console.WriteLine("   ✅ 300 CoworkingSpaces básicos");
            Console.WriteLine("   ✅ BenefitCoworkingSpace (Benefits 1-10)");
            Console.WriteLine("   ✅ CoworkingSpaceSafetyElement (SafetyElements 1-20)");
            Console.WriteLine("   ✅ CoworkingSpaceServiceOffered (Services 1-10)");
            Console.WriteLine("   ✅ CoworkingSpaceSpecialFeature (SpecialFeatures 1-20)");
            Console.WriteLine("   ✅ CoworkingSpacePhotos (1500 fotos: 5 por espacio)");
            Console.WriteLine("   ✅ CoworkingAreas (configuraciones variadas por espacio)");
            Console.WriteLine("   ✅ CoworkingAvailabilities (disponibilidades para 12 meses)");
            Console.WriteLine("   ✅ Reservations (1350-1850 reservas: pasadas, actuales y futuras)");
            Console.WriteLine("   ✅ Reviews (basadas en reservas completadas del pasado)");
        }
    }
}

