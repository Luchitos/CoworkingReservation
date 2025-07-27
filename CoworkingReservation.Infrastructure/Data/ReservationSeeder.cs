using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CoworkingReservation.Infrastructure.Data
{
    public static class ReservationSeeder
    {
        public static async Task SeedReservationsAsync(ApplicationDbContext context)
        {
            // Verificar si ya existen reservas
            var existingReservationsCount = await context.Reservations.CountAsync();
            if (existingReservationsCount > 0)
            {
                Console.WriteLine($"✅ Ya existen {existingReservationsCount} reservas en la base de datos.");
                return;
            }

            Console.WriteLine("🗓️ Iniciando creación de reservas simuladas...");

            // Obtener datos necesarios
            var coworkingSpaces = await context.CoworkingSpaces
                .Where(cs => cs.HosterId >= 1 && cs.HosterId <= 300)
                .Include(cs => cs.Areas)
                .ToListAsync();

            // Obtener todos los usuarios (hosts que también pueden ser clientes de otros espacios)
            var allUsers = await context.Users
                .Where(u => u.Id >= 1 && u.Id <= 300)
                .Select(u => new { u.Id, u.Name, u.Lastname })
                .ToListAsync();

            // Obtener todas las disponibilidades para verificar y actualizar
            var allAvailabilities = await context.CoworkingAvailabilities
                .ToListAsync();

            if (allAvailabilities.Count == 0)
            {
                Console.WriteLine("❌ Error: No se encontraron registros de CoworkingAvailabilities. Ejecute primero el CoworkingAvailabilitySeeder.");
                return;
            }

            // Convertir a lista dinámica para poder usar en los métodos
            var dynamicUsers = allUsers.Cast<dynamic>().ToList();

            if (coworkingSpaces.Count != 300)
            {
                Console.WriteLine($"❌ Error: Se esperaban 300 CoworkingSpaces, pero se encontraron {coworkingSpaces.Count}");
                return;
            }

            if (allUsers.Count == 0)
            {
                Console.WriteLine("❌ Error: No se encontraron usuarios para crear reservas.");
                return;
            }

            var random = new Random(42); // Seed fijo para resultados reproducibles
            var allReservations = new List<Reservation>();
            var allReservationDetails = new List<ReservationDetail>();

            // Generar reservas
            GenerateReservations(coworkingSpaces, dynamicUsers, random, allReservations, allReservationDetails, allAvailabilities);

            // Insertar reservas y detalles en lotes
            await InsertReservationsInBatches(context, allReservations, allReservationDetails);

            // Actualizar disponibilidades basadas en las reservas creadas
            await UpdateAvailabilitiesBasedOnReservations(context, allAvailabilities);

            // Mostrar estadísticas finales
            ShowReservationStatistics(allReservations, allReservationDetails);
        }

        private static void GenerateReservations(
            List<CoworkingSpace> coworkingSpaces,
            List<dynamic> dynamicUsers,
            Random random,
            List<Reservation> allReservations,
            List<ReservationDetail> allReservationDetails,
            List<CoworkingAvailability> allAvailabilities)
        {
            var baseDate = DateTime.Today;
            
            // Generar reservas pasadas (últimos 6 meses)
            GeneratePastReservations(coworkingSpaces, dynamicUsers, random, allReservations, allReservationDetails, baseDate, allAvailabilities);
            
            // Generar reservas actuales (este mes)
            GenerateCurrentReservations(coworkingSpaces, dynamicUsers, random, allReservations, allReservationDetails, baseDate, allAvailabilities);
            
            // Generar reservas futuras (próximos 3 meses)
            GenerateFutureReservations(coworkingSpaces, dynamicUsers, random, allReservations, allReservationDetails, baseDate, allAvailabilities);
        }

        private static void GeneratePastReservations(
            List<CoworkingSpace> coworkingSpaces,
            List<dynamic> dynamicUsers,
            Random random,
            List<Reservation> allReservations,
            List<ReservationDetail> allReservationDetails,
            DateTime baseDate,
            List<CoworkingAvailability> allAvailabilities)
        {
            Console.WriteLine("📈 Generando reservas pasadas (últimos 6 meses)...");
            
            var startDate = baseDate.AddMonths(-6);
            var endDate = baseDate.AddDays(-1);
            
            // Generar aproximadamente 800-1000 reservas pasadas
            var reservationCount = 0;
            var targetReservations = random.Next(800, 1001);
            
            for (var date = startDate; date <= endDate && reservationCount < targetReservations; date = date.AddDays(1))
            {
                // Probabilidad variable según día de la semana
                var dailyProbability = GetDailyReservationProbability(date.DayOfWeek);
                var dailyReservations = (int)(dailyProbability * random.Next(1, 6));
                
                for (int i = 0; i < dailyReservations && reservationCount < targetReservations; i++)
                {
                    var reservation = CreateRandomReservation(
                        coworkingSpaces, dynamicUsers, random, date, 
                        ReservationTimeType.Past, allReservationDetails, allAvailabilities);
                    
                    if (reservation != null)
                    {
                        allReservations.Add(reservation);
                        reservationCount++;
                    }
                }
            }
            
            Console.WriteLine($"✅ Generadas {reservationCount} reservas pasadas");
        }

        private static void GenerateCurrentReservations(
            List<CoworkingSpace> coworkingSpaces,
            List<dynamic> dynamicUsers,
            Random random,
            List<Reservation> allReservations,
            List<ReservationDetail> allReservationDetails,
            DateTime baseDate,
            List<CoworkingAvailability> allAvailabilities)
        {
            Console.WriteLine("📍 Generando reservas actuales (este mes)...");
            
            var startDate = new DateTime(baseDate.Year, baseDate.Month, 1);
            var endDate = baseDate.AddDays(30);
            
            var reservationCount = 0;
            var targetReservations = random.Next(150, 251); // 150-250 reservas activas
            
            for (var date = startDate; date <= endDate && reservationCount < targetReservations; date = date.AddDays(1))
            {
                var dailyProbability = GetDailyReservationProbability(date.DayOfWeek) * 1.5; // Más actividad actual
                var dailyReservations = (int)(dailyProbability * random.Next(2, 8));
                
                for (int i = 0; i < dailyReservations && reservationCount < targetReservations; i++)
                {
                    var reservation = CreateRandomReservation(
                        coworkingSpaces, dynamicUsers, random, date, 
                        ReservationTimeType.Current, allReservationDetails, allAvailabilities);
                    
                    if (reservation != null)
                    {
                        allReservations.Add(reservation);
                        reservationCount++;
                    }
                }
            }
            
            Console.WriteLine($"✅ Generadas {reservationCount} reservas actuales");
        }

        private static void GenerateFutureReservations(
            List<CoworkingSpace> coworkingSpaces,
            List<dynamic> dynamicUsers,
            Random random,
            List<Reservation> allReservations,
            List<ReservationDetail> allReservationDetails,
            DateTime baseDate,
            List<CoworkingAvailability> allAvailabilities)
        {
            Console.WriteLine("📅 Generando reservas futuras (próximos 3 meses)...");
            
            var startDate = baseDate.AddDays(1);
            var endDate = baseDate.AddMonths(3);
            
            var reservationCount = 0;
            var targetReservations = random.Next(400, 601); // 400-600 reservas futuras
            
            for (var date = startDate; date <= endDate && reservationCount < targetReservations; date = date.AddDays(1))
            {
                var dailyProbability = GetDailyReservationProbability(date.DayOfWeek);
                var dailyReservations = (int)(dailyProbability * random.Next(1, 5));
                
                for (int i = 0; i < dailyReservations && reservationCount < targetReservations; i++)
                {
                    var reservation = CreateRandomReservation(
                        coworkingSpaces, dynamicUsers, random, date, 
                        ReservationTimeType.Future, allReservationDetails, allAvailabilities);
                    
                    if (reservation != null)
                    {
                        allReservations.Add(reservation);
                        reservationCount++;
                    }
                }
            }
            
            Console.WriteLine($"✅ Generadas {reservationCount} reservas futuras");
        }

        private static Reservation CreateRandomReservation(
            List<CoworkingSpace> coworkingSpaces,
            List<dynamic> dynamicUsers,
            Random random,
            DateTime baseDate,
            ReservationTimeType timeType,
            List<ReservationDetail> allReservationDetails,
            List<CoworkingAvailability> allAvailabilities)
        {
            // Intentar hasta 10 veces encontrar una combinación válida (usuario != host del espacio)
            for (int attempt = 0; attempt < 10; attempt++)
            {
                // Seleccionar espacio de coworking aleatorio
                var coworkingSpace = coworkingSpaces[random.Next(coworkingSpaces.Count)];
                
                // Verificar que el espacio tenga áreas
                if (!coworkingSpace.Areas.Any())
                    continue;
                
                // Seleccionar usuario aleatorio
                var user = dynamicUsers[random.Next(dynamicUsers.Count)];
                
                // Evitar que un host reserve su propio espacio
                if ((int)user.Id == coworkingSpace.HosterId)
                    continue;
                
                // Determinar duración de la reserva (1-7 días, más probable 1-3)
                var duration = GetReservationDuration(random);
                var startDate = baseDate;
                var endDate = startDate.AddDays(duration - 1);
                
                // Seleccionar áreas aleatorias (1-3 áreas por reserva)
                var selectedAreas = SelectRandomAreas(coworkingSpace.Areas.ToList(), random);
                
                // Calcular precio total
                var totalPrice = CalculateTotalPrice(selectedAreas, duration);
                
                // Determinar estado según el tipo de tiempo
                var status = DetermineReservationStatus(timeType, random);
                
                // Crear reserva
                var reservation = new Reservation
                {
                    UserId = (int)user.Id,
                    CoworkingSpaceId = coworkingSpace.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = status,
                    TotalPrice = totalPrice,
                    PaymentMethod = GetRandomPaymentMethod(random),
                    CreatedAt = GetCreatedAtDate(startDate, timeType, random),
                    UpdatedAt = status == ReservationStatus.Cancelled ? GetUpdatedAtDate(startDate, random) : null,
                    ReservationDetails = new List<ReservationDetail>()
                };
                
                // Crear detalles de reserva
                foreach (var area in selectedAreas)
                {
                    var detail = new ReservationDetail
                    {
                        Reservation = reservation,
                        CoworkingAreaId = area.Id,
                        PricePerDay = area.PricePerDay
                    };
                    
                    reservation.ReservationDetails.Add(detail);
                    allReservationDetails.Add(detail);
                }
                
                // Actualizar disponibilidades basadas en la reserva creada
                UpdateAvailabilitiesBasedOnReservation(reservation, allAvailabilities);
                
                return reservation;
            }
            
            // Si después de 10 intentos no se pudo crear una reserva válida, retornar null
            return null;
        }

        #region Helper Methods
        
        private static double GetDailyReservationProbability(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => 1.0,
                DayOfWeek.Tuesday => 1.2,
                DayOfWeek.Wednesday => 1.3,
                DayOfWeek.Thursday => 1.2,
                DayOfWeek.Friday => 0.9,
                DayOfWeek.Saturday => 0.4,
                DayOfWeek.Sunday => 0.2,
                _ => 1.0
            };
        }
        
        private static int GetReservationDuration(Random random)
        {
            var probability = random.NextDouble();
            return probability switch
            {
                < 0.5 => 1,   // 50% - 1 día
                < 0.75 => 2,  // 25% - 2 días
                < 0.85 => 3,  // 10% - 3 días
                < 0.92 => 4,  // 7% - 4 días
                < 0.96 => 5,  // 4% - 5 días
                < 0.98 => 6,  // 2% - 6 días
                _ => 7        // 2% - 7 días
            };
        }
        
        private static List<CoworkingArea> SelectRandomAreas(List<CoworkingArea> areas, Random random)
        {
            var numberOfAreas = random.NextDouble() switch
            {
                < 0.7 => 1,   // 70% - 1 área
                < 0.9 => 2,   // 20% - 2 áreas
                _ => 3        // 10% - 3 áreas
            };
            
            numberOfAreas = Math.Min(numberOfAreas, areas.Count);
            
            return areas.OrderBy(x => random.Next()).Take(numberOfAreas).ToList();
        }
        
        private static decimal CalculateTotalPrice(List<CoworkingArea> areas, int duration)
        {
            return areas.Sum(a => a.PricePerDay) * duration;
        }
        
        private static ReservationStatus DetermineReservationStatus(ReservationTimeType timeType, Random random)
        {
            return timeType switch
            {
                ReservationTimeType.Past => random.NextDouble() switch
                {
                    < 0.85 => ReservationStatus.Completed,  // 85% completadas
                    < 0.95 => ReservationStatus.Cancelled,  // 10% canceladas
                    _ => ReservationStatus.Confirmed        // 5% confirmadas (casos especiales)
                },
                ReservationTimeType.Current => random.NextDouble() switch
                {
                    < 0.90 => ReservationStatus.Confirmed,  // 90% confirmadas
                    < 0.95 => ReservationStatus.Pending,    // 5% pendientes
                    _ => ReservationStatus.Cancelled        // 5% canceladas
                },
                ReservationTimeType.Future => random.NextDouble() switch
                {
                    < 0.70 => ReservationStatus.Confirmed,  // 70% confirmadas
                    < 0.85 => ReservationStatus.Pending,    // 15% pendientes
                    _ => ReservationStatus.Cancelled        // 15% canceladas
                },
                _ => ReservationStatus.Pending
            };
        }
        
        private static PaymentMethodType GetRandomPaymentMethod(Random random)
        {
            return random.NextDouble() switch
            {
                < 0.6 => PaymentMethodType.CreditCard,  // 60%
                < 0.9 => PaymentMethodType.DebitCard,   // 30%
                _ => PaymentMethodType.Cash             // 10%
            };
        }
        
        private static DateTime GetCreatedAtDate(DateTime reservationDate, ReservationTimeType timeType, Random random)
        {
            return timeType switch
            {
                ReservationTimeType.Past => reservationDate.AddDays(-random.Next(1, 30)),     // Creada 1-30 días antes
                ReservationTimeType.Current => reservationDate.AddDays(-random.Next(1, 15)),  // Creada 1-15 días antes
                ReservationTimeType.Future => DateTime.UtcNow.AddDays(-random.Next(0, 7)),    // Creada hace 0-7 días
                _ => DateTime.UtcNow
            };
        }
        
        private static DateTime? GetUpdatedAtDate(DateTime reservationDate, Random random)
        {
            return reservationDate.AddDays(-random.Next(1, 10)); // Cancelada 1-10 días antes de la reserva
        }
        
        private static void UpdateAvailabilitiesBasedOnReservation(Reservation reservation, List<CoworkingAvailability> allAvailabilities)
        {
            // Actualizar disponibilidades para cada día de la reserva
            for (var date = reservation.StartDate.Date; date <= reservation.EndDate.Date; date = date.AddDays(1))
            {
                foreach (var detail in reservation.ReservationDetails)
                {
                    var availability = allAvailabilities.FirstOrDefault(a => 
                        a.CoworkingAreaId == detail.CoworkingAreaId && 
                        a.Date.Date == date);
                    
                    if (availability != null && availability.AvailableSpots > 0)
                    {
                        availability.AvailableSpots--; // Reducir spots disponibles
                    }
                }
            }
        }
        
        #endregion

        private static async Task InsertReservationsInBatches(
            ApplicationDbContext context,
            List<Reservation> allReservations,
            List<ReservationDetail> allReservationDetails)
        {
            Console.WriteLine("💾 Insertando reservas en la base de datos...");
            
            var batchSize = 50;
            var processedCount = 0;
            
            // Insertar reservas en lotes
            for (int i = 0; i < allReservations.Count; i += batchSize)
            {
                var batch = allReservations.Skip(i).Take(batchSize);
                await context.Reservations.AddRangeAsync(batch);
                await context.SaveChangesAsync();
                
                processedCount += batch.Count();
                if (processedCount % 100 == 0)
                {
                    Console.WriteLine($"✅ Procesadas {processedCount}/{allReservations.Count} reservas...");
                }
            }
            
            Console.WriteLine($"🎉 ¡{allReservations.Count} reservas creadas exitosamente!");
        }

        private static void ShowReservationStatistics(List<Reservation> allReservations, List<ReservationDetail> allReservationDetails)
        {
            var stats = allReservations.GroupBy(r => r.Status).ToDictionary(g => g.Key, g => g.Count());
            var paymentStats = allReservations.GroupBy(r => r.PaymentMethod).ToDictionary(g => g.Key, g => g.Count());
            var totalRevenue = allReservations.Where(r => r.Status != ReservationStatus.Cancelled)
                                            .Sum(r => r.TotalPrice);

            Console.WriteLine($"📊 Estadísticas de reservas creadas:");
            Console.WriteLine($"   📈 Total de reservas: {allReservations.Count}");
            Console.WriteLine($"   ✅ Confirmadas: {stats.GetValueOrDefault(ReservationStatus.Confirmed, 0)}");
            Console.WriteLine($"   ⏳ Pendientes: {stats.GetValueOrDefault(ReservationStatus.Pending, 0)}");
            Console.WriteLine($"   ✔️ Completadas: {stats.GetValueOrDefault(ReservationStatus.Completed, 0)}");
            Console.WriteLine($"   ❌ Canceladas: {stats.GetValueOrDefault(ReservationStatus.Cancelled, 0)}");
            Console.WriteLine($"   💳 Pago con tarjeta de crédito: {paymentStats.GetValueOrDefault(PaymentMethodType.CreditCard, 0)}");
            Console.WriteLine($"   💳 Pago con tarjeta de débito: {paymentStats.GetValueOrDefault(PaymentMethodType.DebitCard, 0)}");
            Console.WriteLine($"   💵 Pago en efectivo: {paymentStats.GetValueOrDefault(PaymentMethodType.Cash, 0)}");
            Console.WriteLine($"   💰 Ingresos totales (sin canceladas): ${totalRevenue:N2}");
        }

        private static async Task UpdateAvailabilitiesBasedOnReservations(ApplicationDbContext context, List<CoworkingAvailability> allAvailabilities)
        {
            Console.WriteLine("🔄 Actualizando disponibilidades en la base de datos...");
            
            var batchSize = 1000;
            var processedCount = 0;
            
            for (int i = 0; i < allAvailabilities.Count; i += batchSize)
            {
                var batch = allAvailabilities.Skip(i).Take(batchSize);
                context.CoworkingAvailabilities.UpdateRange(batch);
                await context.SaveChangesAsync();
                
                processedCount += batch.Count();
                
                if (processedCount % 5000 == 0 || processedCount == allAvailabilities.Count)
                {
                    Console.WriteLine($"✅ Actualizadas {processedCount}/{allAvailabilities.Count} disponibilidades...");
                }
            }
            
            Console.WriteLine("🎉 ¡Disponibilidades actualizadas correctamente!");
        }
    }

    public enum ReservationTimeType
    {
        Past,
        Current,
        Future
    }
} 