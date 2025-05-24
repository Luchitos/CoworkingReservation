using Microsoft.EntityFrameworkCore;

namespace CoworkingReservation.Infrastructure.Data
{
    public static class BenefitCoworkingSpaceSeeder
    {
        public static async Task SeedBenefitCoworkingSpaceAsync(ApplicationDbContext context)
        {
            Console.WriteLine("🔗 Iniciando seeder de BenefitCoworkingSpace...");

            try
            {
                // Verificar si ya existen datos en la tabla de unión
                var existingCount = await context.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(*) as Value FROM BenefitCoworkingSpace").FirstAsync();

                if (existingCount > 0)
                {
                    Console.WriteLine($"✅ La tabla BenefitCoworkingSpace ya tiene {existingCount} registros.");
                    return;
                }

                // Verificar que existan los CoworkingSpaces y Benefits necesarios
                var coworkingSpacesCount = await context.CoworkingSpaces.CountAsync(cs => cs.Id >= 1 && cs.Id <= 300);
                var benefitsCount = await context.Benefits.CountAsync(b => b.Id >= 1 && b.Id <= 10);

                Console.WriteLine($"📊 Verificando datos existentes:");
                Console.WriteLine($"   CoworkingSpaces encontrados (IDs 1-300): {coworkingSpacesCount}");
                Console.WriteLine($"   Benefits encontrados (IDs 1-10): {benefitsCount}");

                if (coworkingSpacesCount < 300)
                {
                    Console.WriteLine("❌ No se encontraron suficientes CoworkingSpaces (necesarios: 300).");
                    return;
                }

                if (benefitsCount < 10)
                {
                    Console.WriteLine("❌ No se encontraron suficientes Benefits (necesarios: 10).");
                    return;
                }

                Console.WriteLine("🔗 Generando relaciones BenefitCoworkingSpace...");

                var random = new Random(42); // Seed fijo para reproducibilidad
                var insertStatements = new List<string>();

                // Para cada CoworkingSpace (1-300)
                for (int coworkingSpaceId = 1; coworkingSpaceId <= 300; coworkingSpaceId++)
                {
                    // Generar entre 3 y 10 benefits aleatorios
                    var benefitCount = random.Next(3, 11); // 3 a 10 inclusive
                    
                    // Seleccionar benefits únicos para este CoworkingSpace
                    var selectedBenefits = Enumerable.Range(1, 10) // Benefits con IDs 1-10
                        .OrderBy(x => random.Next())
                        .Take(benefitCount)
                        .ToList();

                    // Crear statements de INSERT para cada benefit
                    foreach (var benefitId in selectedBenefits)
                    {
                        insertStatements.Add($"({benefitId}, {coworkingSpaceId})");
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
                    var sql = $"INSERT INTO BenefitCoworkingSpace (BenefitsId, CoworkingSpacesId) VALUES {valuesClause}";

                    await context.Database.ExecuteSqlRawAsync(sql);
                    
                    Console.WriteLine($"   Lote {batch + 1}/{totalBatches} insertado ({batchStatements.Count} registros)");
                }

                Console.WriteLine("🎉 ¡BenefitCoworkingSpace seeding completado exitosamente!");
                
                // Mostrar estadísticas finales
                var finalCount = await context.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(*) as Value FROM BenefitCoworkingSpace").FirstAsync();
                Console.WriteLine($"📊 Total de relaciones creadas: {finalCount}");
                Console.WriteLine($"📊 Promedio de benefits por CoworkingSpace: {finalCount / 300.0:F1}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en BenefitCoworkingSpaceSeeder: {ex.Message}");
                Console.WriteLine($"🔍 Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
} 