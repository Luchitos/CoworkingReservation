using Microsoft.EntityFrameworkCore;

namespace CoworkingReservation.Infrastructure.Data
{
    public static class CoworkingSpaceSpecialFeatureSeeder
    {
        public static async Task SeedCoworkingSpaceSpecialFeatureAsync(ApplicationDbContext context)
        {
            Console.WriteLine("🔗 Iniciando seeder de CoworkingSpaceSpecialFeature...");

            try
            {
                // Verificar si ya existen datos en la tabla de unión
                var existingCount = await context.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(*) as Value FROM CoworkingSpaceSpecialFeature").FirstAsync();

                if (existingCount > 0)
                {
                    Console.WriteLine($"✅ La tabla CoworkingSpaceSpecialFeature ya tiene {existingCount} registros.");
                    return;
                }

                // Verificar que existan los CoworkingSpaces y SpecialFeatures necesarios
                var coworkingSpacesCount = await context.CoworkingSpaces.CountAsync(cs => cs.Id >= 1 && cs.Id <= 300);
                var specialFeaturesCount = await context.SpecialFeatures.CountAsync(sf => sf.Id >= 1 && sf.Id <= 20);

                Console.WriteLine($"📊 Verificando datos existentes:");
                Console.WriteLine($"   CoworkingSpaces encontrados (IDs 1-300): {coworkingSpacesCount}");
                Console.WriteLine($"   SpecialFeatures encontrados (IDs 1-20): {specialFeaturesCount}");

                if (coworkingSpacesCount < 300)
                {
                    Console.WriteLine("❌ No se encontraron suficientes CoworkingSpaces (necesarios: 300).");
                    return;
                }

                if (specialFeaturesCount < 20)
                {
                    Console.WriteLine("❌ No se encontraron suficientes SpecialFeatures (necesarios: 20).");
                    return;
                }

                Console.WriteLine("🔗 Generando relaciones CoworkingSpaceSpecialFeature...");

                var random = new Random(789); // Seed diferente para diferentes resultados
                var insertStatements = new List<string>();

                // Para cada CoworkingSpace (1-300)
                for (int coworkingSpaceId = 1; coworkingSpaceId <= 300; coworkingSpaceId++)
                {
                    // Generar entre 2 y 5 special features aleatorios (menos que otros ya que son características especiales)
                    var specialFeatureCount = random.Next(2, 6); // 2 a 5 inclusive
                    
                    // Seleccionar special features únicos para este CoworkingSpace
                    var selectedSpecialFeatures = Enumerable.Range(1, 20) // SpecialFeatures con IDs 1-20
                        .OrderBy(x => random.Next())
                        .Take(specialFeatureCount)
                        .ToList();

                    // Crear statements de INSERT para cada special feature
                    foreach (var specialFeatureId in selectedSpecialFeatures)
                    {
                        insertStatements.Add($"({coworkingSpaceId}, {specialFeatureId})");
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
                    var sql = $"INSERT INTO CoworkingSpaceSpecialFeature (CoworkingSpacesId, SpecialFeaturesId) VALUES {valuesClause}";

                    await context.Database.ExecuteSqlRawAsync(sql);
                    
                    Console.WriteLine($"   Lote {batch + 1}/{totalBatches} insertado ({batchStatements.Count} registros)");
                }

                Console.WriteLine("🎉 ¡CoworkingSpaceSpecialFeature seeding completado exitosamente!");
                
                // Mostrar estadísticas finales
                var finalCount = await context.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(*) as Value FROM CoworkingSpaceSpecialFeature").FirstAsync();
                Console.WriteLine($"📊 Total de relaciones creadas: {finalCount}");
                Console.WriteLine($"📊 Promedio de special features por CoworkingSpace: {finalCount / 300.0:F1}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en CoworkingSpaceSpecialFeatureSeeder: {ex.Message}");
                Console.WriteLine($"🔍 Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
} 