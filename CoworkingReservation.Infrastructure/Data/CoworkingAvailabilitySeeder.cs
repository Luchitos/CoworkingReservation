using CoworkingReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoworkingReservation.Infrastructure.Data
{
    public static class CoworkingAvailabilitySeeder
    {
        public static async Task SeedCoworkingAvailabilitiesAsync(ApplicationDbContext context)
        {
            // Verificar si ya existen disponibilidades
            var existingAvailabilitiesCount = await context.CoworkingAvailabilities.CountAsync();
            if (existingAvailabilitiesCount > 0)
            {
                Console.WriteLine($"✅ Ya existen {existingAvailabilitiesCount} registros de disponibilidad en la base de datos.");
                return;
            }

            Console.WriteLine("📅 Iniciando creación de disponibilidades para CoworkingAreas...");

            // Obtener todas las áreas de coworking
            var coworkingAreas = await context.CoworkingAreas
                .Where(ca => ca.Available)
                .ToListAsync();

            if (coworkingAreas.Count == 0)
            {
                Console.WriteLine("❌ Error: No se encontraron CoworkingAreas disponibles para crear disponibilidades.");
                return;
            }

            Console.WriteLine($"📍 Encontradas {coworkingAreas.Count} áreas de coworking disponibles");

            // Definir el rango de fechas (6 meses hacia atrás y 6 meses hacia adelante)
            var startDate = DateTime.Today.AddMonths(-6);
            var endDate = DateTime.Today.AddMonths(6);
            var totalDays = (endDate - startDate).Days + 1;

            Console.WriteLine($"📆 Generando disponibilidades desde {startDate:yyyy-MM-dd} hasta {endDate:yyyy-MM-dd} ({totalDays} días)");

            var allAvailabilities = new List<CoworkingAvailability>();
            var totalRecordsExpected = coworkingAreas.Count * totalDays;

            // Generar disponibilidades para cada área y cada día
            foreach (var area in coworkingAreas)
            {
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var availability = new CoworkingAvailability
                    {
                        CoworkingAreaId = area.Id,
                        Date = date,
                        AvailableSpots = area.Capacity // Inicialmente toda la capacidad está disponible
                    };

                    allAvailabilities.Add(availability);
                }
            }

            Console.WriteLine($"🔢 Registros de disponibilidad generados: {allAvailabilities.Count} (esperados: {totalRecordsExpected})");

            // Insertar en lotes para mejor rendimiento
            await InsertAvailabilitiesInBatches(context, allAvailabilities);

            // Mostrar estadísticas finales
            ShowAvailabilityStatistics(coworkingAreas, totalDays, allAvailabilities.Count);
        }

        private static async Task InsertAvailabilitiesInBatches(
            ApplicationDbContext context, 
            List<CoworkingAvailability> allAvailabilities)
        {
            Console.WriteLine("💾 Insertando disponibilidades en la base de datos...");
            
            var batchSize = 1000; // Lotes más grandes para availabilities
            var processedCount = 0;
            
            for (int i = 0; i < allAvailabilities.Count; i += batchSize)
            {
                var batch = allAvailabilities.Skip(i).Take(batchSize);
                await context.CoworkingAvailabilities.AddRangeAsync(batch);
                await context.SaveChangesAsync();
                
                processedCount += batch.Count();
                
                // Mostrar progreso cada 5000 registros
                if (processedCount % 5000 == 0 || processedCount == allAvailabilities.Count)
                {
                    Console.WriteLine($"✅ Procesadas {processedCount}/{allAvailabilities.Count} disponibilidades...");
                }
            }
            
            Console.WriteLine($"🎉 ¡{allAvailabilities.Count} registros de disponibilidad creados exitosamente!");
        }

        private static void ShowAvailabilityStatistics(
            List<CoworkingArea> coworkingAreas, 
            int totalDays, 
            int totalRecords)
        {
            var areaTypeStats = coworkingAreas.GroupBy(a => a.Type).ToDictionary(g => g.Key, g => g.Count());
            var totalCapacity = coworkingAreas.Sum(a => a.Capacity);
            var avgCapacityPerArea = coworkingAreas.Average(a => a.Capacity);

            Console.WriteLine($"📊 Estadísticas de disponibilidades creadas:");
            Console.WriteLine($"   🏢 Total áreas procesadas: {coworkingAreas.Count}");
            Console.WriteLine($"   📅 Días cubiertos: {totalDays}");
            Console.WriteLine($"   📈 Total registros creados: {totalRecords}");
            Console.WriteLine($"   🎯 Capacidad total: {totalCapacity} spots");
            Console.WriteLine($"   📊 Capacidad promedio por área: {avgCapacityPerArea:F1} spots");
            
            Console.WriteLine($"   📋 Distribución por tipo de área:");
            foreach (var stat in areaTypeStats)
            {
                Console.WriteLine($"      - {stat.Key}: {stat.Value} áreas");
            }
            
            Console.WriteLine($"   🔢 Spots totales disponibles por día: {totalCapacity}");
            Console.WriteLine($"   🔢 Spots totales en el período: {totalCapacity * totalDays:N0}");
        }
    }
} 