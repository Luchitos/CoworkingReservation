using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoworkingReservation.Infrastructure.Data
{
    public static class CoworkingAreaSeeder
    {
        public static async Task SeedCoworkingAreasAsync(ApplicationDbContext context)
        {
            // Verificar si ya existen CoworkingAreas
            var existingAreasCount = await context.CoworkingAreas.CountAsync();
            if (existingAreasCount > 0)
            {
                Console.WriteLine($"✅ Ya existen {existingAreasCount} CoworkingAreas en la base de datos.");
                return;
            }

            Console.WriteLine("🏢 Iniciando creación de CoworkingAreas para los 300 espacios...");

            // Obtener todos los CoworkingSpaces existentes
            var coworkingSpaces = await context.CoworkingSpaces
                .Where(cs => cs.HosterId >= 1 && cs.HosterId <= 300)
                .Select(cs => new { cs.Id, cs.CapacityTotal, cs.Name })
                .ToListAsync();

            if (coworkingSpaces.Count != 300)
            {
                Console.WriteLine($"❌ Error: Se esperaban 300 CoworkingSpaces, pero se encontraron {coworkingSpaces.Count}");
                return;
            }

            var random = new Random(42); // Seed fijo para resultados reproducibles
            var allAreas = new List<CoworkingArea>();
            var processedCount = 0;

            foreach (var space in coworkingSpaces)
            {
                var areas = CreateAreasForSpace(space.Id, space.CapacityTotal, random);
                allAreas.AddRange(areas);
                
                processedCount++;
                if (processedCount % 50 == 0)
                {
                    Console.WriteLine($"✅ Configuradas áreas para {processedCount}/300 espacios...");
                }
            }

            // Insertar todas las áreas en lotes para mejor rendimiento
            var batchSize = 100;
            for (int i = 0; i < allAreas.Count; i += batchSize)
            {
                var batch = allAreas.Skip(i).Take(batchSize);
                await context.CoworkingAreas.AddRangeAsync(batch);
                await context.SaveChangesAsync();
            }

            Console.WriteLine($"🎉 ¡{allAreas.Count} CoworkingAreas creadas exitosamente!");
            
            // Estadísticas finales
            var stats = CalculateStatistics(allAreas);
            Console.WriteLine($"📊 Estadísticas de áreas creadas:");
            Console.WriteLine($"   • Espacios solo con escritorios individuales: {stats.IndividualOnlySpaces}");
            Console.WriteLine($"   • Espacios solo con oficinas privadas: {stats.PrivateOfficeOnlySpaces}");
            Console.WriteLine($"   • Espacios híbridos: {stats.HybridSpaces}");
            Console.WriteLine($"   • Total de escritorios individuales: {stats.TotalIndividualDesks}");
            Console.WriteLine($"   • Total de oficinas privadas: {stats.TotalPrivateOffices}");
        }

        private static List<CoworkingArea> CreateAreasForSpace(int coworkingSpaceId, int totalCapacity, Random random)
        {
            var areas = new List<CoworkingArea>();
            
            // Definir tipo de configuración del espacio basado en su capacidad
            var spaceType = DetermineSpaceType(totalCapacity, random);
            
            switch (spaceType)
            {
                case SpaceConfigurationType.IndividualOnly:
                    areas.AddRange(CreateIndividualOnlyAreas(coworkingSpaceId, totalCapacity, random));
                    break;
                    
                case SpaceConfigurationType.PrivateOfficeOnly:
                    areas.AddRange(CreatePrivateOfficeOnlyAreas(coworkingSpaceId, totalCapacity, random));
                    break;
                    
                case SpaceConfigurationType.Hybrid:
                    areas.AddRange(CreateHybridAreas(coworkingSpaceId, totalCapacity, random));
                    break;
            }
            
            return areas;
        }

        private static SpaceConfigurationType DetermineSpaceType(int totalCapacity, Random random)
        {
            // Lógica de distribución basada en capacidad:
            // - Espacios pequeños (10-30): 70% Individual, 20% Híbrido, 10% Oficinas
            // - Espacios medianos (31-60): 40% Individual, 40% Híbrido, 20% Oficinas  
            // - Espacios grandes (61+): 20% Individual, 50% Híbrido, 30% Oficinas
            
            double probability = random.NextDouble();
            
            if (totalCapacity <= 30)
            {
                if (probability < 0.7) return SpaceConfigurationType.IndividualOnly;
                if (probability < 0.9) return SpaceConfigurationType.Hybrid;
                return SpaceConfigurationType.PrivateOfficeOnly;
            }
            else if (totalCapacity <= 60)
            {
                if (probability < 0.4) return SpaceConfigurationType.IndividualOnly;
                if (probability < 0.8) return SpaceConfigurationType.Hybrid;
                return SpaceConfigurationType.PrivateOfficeOnly;
            }
            else
            {
                if (probability < 0.2) return SpaceConfigurationType.IndividualOnly;
                if (probability < 0.7) return SpaceConfigurationType.Hybrid;
                return SpaceConfigurationType.PrivateOfficeOnly;
            }
        }

        private static List<CoworkingArea> CreateIndividualOnlyAreas(int coworkingSpaceId, int totalCapacity, Random random)
        {
            var areas = new List<CoworkingArea>();
            var remainingCapacity = totalCapacity;
            
            // Crear varias áreas de escritorios individuales
            var numberOfAreas = Math.Min(random.Next(2, 5), totalCapacity); // 2-4 áreas máximo
            
            for (int i = 0; i < numberOfAreas && remainingCapacity > 0; i++)
            {
                var isLastArea = (i == numberOfAreas - 1);
                var areaCapacity = isLastArea ? remainingCapacity : random.Next(1, Math.Min(remainingCapacity, 15));
                
                var area = new CoworkingArea
                {
                    CoworkingSpaceId = coworkingSpaceId,
                    Type = CoworkingAreaType.IndividualDesk,
                    Description = GetIndividualDeskDescription(i + 1),
                    Capacity = areaCapacity,
                    PricePerDay = GenerateIndividualDeskPrice(random),
                    Available = true
                };
                
                areas.Add(area);
                remainingCapacity -= areaCapacity;
            }
            
            return areas;
        }

        private static List<CoworkingArea> CreatePrivateOfficeOnlyAreas(int coworkingSpaceId, int totalCapacity, Random random)
        {
            var areas = new List<CoworkingArea>();
            var remainingCapacity = totalCapacity;
            
            // Crear oficinas privadas de diferentes tamaños
            var officeTypes = new[] { 2, 4, 6, 8, 10, 12 }; // Capacidades típicas de oficinas
            
            while (remainingCapacity > 0)
            {
                // Elegir tamaño de oficina que no exceda la capacidad restante
                var availableOfficeTypes = officeTypes.Where(size => size <= remainingCapacity).ToArray();
                if (!availableOfficeTypes.Any()) break;
                
                var officeSize = availableOfficeTypes[random.Next(availableOfficeTypes.Length)];
                
                var area = new CoworkingArea
                {
                    CoworkingSpaceId = coworkingSpaceId,
                    Type = CoworkingAreaType.PrivateOffice,
                    Description = GetPrivateOfficeDescription(officeSize),
                    Capacity = officeSize,
                    PricePerDay = GeneratePrivateOfficePrice(officeSize, random),
                    Available = true
                };
                
                areas.Add(area);
                remainingCapacity -= officeSize;
            }
            
            return areas;
        }

        private static List<CoworkingArea> CreateHybridAreas(int coworkingSpaceId, int totalCapacity, Random random)
        {
            var areas = new List<CoworkingArea>();
            var remainingCapacity = totalCapacity;
            
            // Dividir capacidad entre diferentes tipos (60% individual, 40% oficinas)
            var officeCapacity = (int)(totalCapacity * 0.4);
            var individualCapacity = totalCapacity - officeCapacity;
            
            // Crear oficinas privadas
            var officeTypes = new[] { 2, 4, 6, 8 };
            var usedOfficeCapacity = 0;
            
            while (usedOfficeCapacity < officeCapacity && remainingCapacity > 0)
            {
                var maxOfficeSize = Math.Min(officeCapacity - usedOfficeCapacity, remainingCapacity);
                var availableOfficeTypes = officeTypes.Where(size => size <= maxOfficeSize).ToArray();
                if (!availableOfficeTypes.Any()) break;
                
                var officeSize = availableOfficeTypes[random.Next(availableOfficeTypes.Length)];
                
                areas.Add(new CoworkingArea
                {
                    CoworkingSpaceId = coworkingSpaceId,
                    Type = CoworkingAreaType.PrivateOffice,
                    Description = GetPrivateOfficeDescription(officeSize),
                    Capacity = officeSize,
                    PricePerDay = GeneratePrivateOfficePrice(officeSize, random),
                    Available = true
                });
                
                usedOfficeCapacity += officeSize;
                remainingCapacity -= officeSize;
            }
            
            // Crear áreas de escritorios individuales con la capacidad restante
            while (remainingCapacity > 0)
            {
                var areaCapacity = Math.Min(remainingCapacity, random.Next(1, 8));
                
                areas.Add(new CoworkingArea
                {
                    CoworkingSpaceId = coworkingSpaceId,
                    Type = CoworkingAreaType.IndividualDesk,
                    Description = GetIndividualDeskDescription(areas.Count(a => a.Type == CoworkingAreaType.IndividualDesk) + 1),
                    Capacity = areaCapacity,
                    PricePerDay = GenerateIndividualDeskPrice(random),
                    Available = true
                });
                
                remainingCapacity -= areaCapacity;
            }
            
            return areas;
        }

        #region Description Generators
        
        private static string GetIndividualDeskDescription(int areaNumber)
        {
            var descriptions = new[]
            {
                $"Escritorio individual {areaNumber} con excelente iluminación natural y vista panorámica",
                $"Zona de trabajo personal {areaNumber} equipada con mobiliario ergonómico",
                $"Espacio individual {areaNumber} silencioso, ideal para concentración y productividad",
                $"Escritorio privado {areaNumber} con acceso directo a áreas comunes",
                $"Puesto de trabajo individual {areaNumber} en ambiente moderno y profesional"
            };
            
            return descriptions[areaNumber % descriptions.Length];
        }
        
        private static string GetPrivateOfficeDescription(int capacity)
        {
            return $"Oficina privada para {capacity} personas con ventanas, climatización independiente y total privacidad";
        }
        
        #endregion

        #region Price Generators
        
        private static decimal GenerateIndividualDeskPrice(Random random)
        {
            // Precios entre $800 y $2,500 pesos argentinos por día
            return Math.Round((decimal)(random.NextDouble() * 1700 + 800), 2);
        }
        
        private static decimal GeneratePrivateOfficePrice(int capacity, Random random)
        {
            // Precio base + precio por persona
            var basePrice = random.NextDouble() * 2000 + 3000; // Base entre $3,000-$5,000
            var perPersonPrice = random.NextDouble() * 1000 + 800; // $800-$1,800 por persona
            
            return Math.Round((decimal)(basePrice + (capacity * perPersonPrice)), 2);
        }
        
        #endregion

        private static (int IndividualOnlySpaces, int PrivateOfficeOnlySpaces, int HybridSpaces, 
                       int TotalIndividualDesks, int TotalPrivateOffices) CalculateStatistics(List<CoworkingArea> allAreas)
        {
            var spaceGroups = allAreas.GroupBy(a => a.CoworkingSpaceId);
            
            var individualOnlySpaces = 0;
            var privateOfficeOnlySpaces = 0;
            var hybridSpaces = 0;
            
            foreach (var spaceGroup in spaceGroups)
            {
                var areaTypes = spaceGroup.Select(a => a.Type).Distinct().ToList();
                
                if (areaTypes.Count == 1)
                {
                    if (areaTypes[0] == CoworkingAreaType.IndividualDesk)
                        individualOnlySpaces++;
                    else if (areaTypes[0] == CoworkingAreaType.PrivateOffice)
                        privateOfficeOnlySpaces++;
                }
                else
                {
                    hybridSpaces++;
                }
            }
            
            return (
                individualOnlySpaces,
                privateOfficeOnlySpaces, 
                hybridSpaces,
                allAreas.Count(a => a.Type == CoworkingAreaType.IndividualDesk),
                allAreas.Count(a => a.Type == CoworkingAreaType.PrivateOffice)
            );
        }
    }

    public enum SpaceConfigurationType
    {
        IndividualOnly,
        PrivateOfficeOnly,
        Hybrid
    }
} 