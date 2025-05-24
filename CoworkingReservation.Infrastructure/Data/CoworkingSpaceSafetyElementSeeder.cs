using Microsoft.EntityFrameworkCore;

namespace CoworkingReservation.Infrastructure.Data
{
    public static class CoworkingSpaceSafetyElementSeeder
    {
        public static async Task SeedCoworkingSpaceSafetyElementAsync(ApplicationDbContext context)
        {
            Console.WriteLine("🔗 Iniciando seeder de CoworkingSpaceSafetyElement...");

            try
            {
                // Verificar si ya existen datos en la tabla de unión
                var existingCount = await context.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(*) as Value FROM CoworkingSpaceSafetyElement").FirstAsync();

                if (existingCount > 0)
                {
                    Console.WriteLine($"✅ La tabla CoworkingSpaceSafetyElement ya tiene {existingCount} registros.");
                    return;
                }

                // Verificar que existan los CoworkingSpaces y SafetyElements necesarios
                var coworkingSpacesCount = await context.CoworkingSpaces.CountAsync(cs => cs.Id >= 1 && cs.Id <= 300);
                var safetyElementsCount = await context.SafetyElements.CountAsync(se => se.Id >= 1 && se.Id <= 20);

                Console.WriteLine($"📊 Verificando datos existentes:");
                Console.WriteLine($"   CoworkingSpaces encontrados (IDs 1-300): {coworkingSpacesCount}");
                Console.WriteLine($"   SafetyElements encontrados (IDs 1-20): {safetyElementsCount}");

                if (coworkingSpacesCount < 300)
                {
                    Console.WriteLine("❌ No se encontraron suficientes CoworkingSpaces (necesarios: 300).");
                    return;
                }

                if (safetyElementsCount < 20)
                {
                    Console.WriteLine("❌ No se encontraron suficientes SafetyElements (necesarios: 20).");
                    return;
                }

                Console.WriteLine("🔗 Generando relaciones CoworkingSpaceSafetyElement...");

                var random = new Random(123); // Seed diferente para diferentes resultados
                var insertStatements = new List<string>();

                // Para cada CoworkingSpace (1-300)
                for (int coworkingSpaceId = 1; coworkingSpaceId <= 300; coworkingSpaceId++)
                {
                    // Generar entre 3 y 8 safety elements aleatorios (menos que benefits)
                    var safetyElementCount = random.Next(3, 9); // 3 a 8 inclusive
                    
                    // Seleccionar safety elements únicos para este CoworkingSpace
                    var selectedSafetyElements = Enumerable.Range(1, 20) // SafetyElements con IDs 1-20
                        .OrderBy(x => random.Next())
                        .Take(safetyElementCount)
                        .ToList();

                    // Crear statements de INSERT para cada safety element
                    foreach (var safetyElementId in selectedSafetyElements)
                    {
                        insertStatements.Add($"({coworkingSpaceId}, {safetyElementId})");
                    }
                }

                Console.WriteLine($"📝 Preparando inserción de {insertStatements.Count} relaciones...");

                // Insertar por lotes de 100 para evitar problemas de memoria
                var batchSize = 100;
                var totalBatches = (int)Math.Ceiling((double)insertStatements.Count / batchSize);

                for (int batch = 0; batch < totalBatches; batch++)
                {
                    var batchStatements = insertStatements
                        .Skip(batch * batchSize)
                        .Take(batchSize)
                        .ToList();

                    var valuesClause = string.Join(", ", batchStatements);
                    var sql = $"INSERT INTO CoworkingSpaceSafetyElement (coworkingSpacesId, SafetyElementsId) VALUES {valuesClause}";

                    await context.Database.ExecuteSqlRawAsync(sql);
                    
                    Console.WriteLine($"   Lote {batch + 1}/{totalBatches} insertado ({batchStatements.Count} registros)");
                }

                Console.WriteLine("🎉 ¡CoworkingSpaceSafetyElement seeding completado exitosamente!");
                
                // Mostrar estadísticas finales
                var finalCount = await context.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(*) as Value FROM CoworkingSpaceSafetyElement").FirstAsync();
                Console.WriteLine($"📊 Total de relaciones creadas: {finalCount}");
                Console.WriteLine($"📊 Promedio de safety elements por CoworkingSpace: {finalCount / 300.0:F1}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en CoworkingSpaceSafetyElementSeeder: {ex.Message}");
                Console.WriteLine($"🔍 Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
} 