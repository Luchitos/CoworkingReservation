using Microsoft.EntityFrameworkCore;

namespace CoworkingReservation.Infrastructure.Data
{
    public static class CoworkingSpaceServiceOfferedSeeder
    {
        public static async Task SeedCoworkingSpaceServiceOfferedAsync(ApplicationDbContext context)
        {
            Console.WriteLine("🔗 Iniciando seeder de CoworkingSpaceServiceOffered...");

            try
            {
                // Verificar si ya existen datos en la tabla de unión
                var existingCount = await context.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(*) as Value FROM CoworkingSpaceServiceOffered").FirstAsync();

                if (existingCount > 0)
                {
                    Console.WriteLine($"✅ La tabla CoworkingSpaceServiceOffered ya tiene {existingCount} registros.");
                    return;
                }

                // Verificar que existan los CoworkingSpaces y ServicesOffered necesarios
                var coworkingSpacesCount = await context.CoworkingSpaces.CountAsync(cs => cs.Id >= 1 && cs.Id <= 300);
                var servicesCount = await context.ServicesOffered.CountAsync(so => so.Id >= 1 && so.Id <= 10);

                Console.WriteLine($"📊 Verificando datos existentes:");
                Console.WriteLine($"   CoworkingSpaces encontrados (IDs 1-300): {coworkingSpacesCount}");
                Console.WriteLine($"   ServicesOffered encontrados (IDs 1-10): {servicesCount}");

                if (coworkingSpacesCount < 300)
                {
                    Console.WriteLine("❌ No se encontraron suficientes CoworkingSpaces (necesarios: 300).");
                    return;
                }

                if (servicesCount < 10)
                {
                    Console.WriteLine("❌ No se encontraron suficientes ServicesOffered (necesarios: 10).");
                    return;
                }

                Console.WriteLine("🔗 Generando relaciones CoworkingSpaceServiceOffered...");

                var random = new Random(456); // Seed diferente para diferentes resultados
                var insertStatements = new List<string>();

                // Para cada CoworkingSpace (1-300)
                for (int coworkingSpaceId = 1; coworkingSpaceId <= 300; coworkingSpaceId++)
                {
                    // Generar entre 4 y 8 services aleatorios
                    var serviceCount = random.Next(4, 9); // 4 a 8 inclusive
                    
                    // Seleccionar services únicos para este CoworkingSpace
                    var selectedServices = Enumerable.Range(1, 10) // ServicesOffered con IDs 1-10
                        .OrderBy(x => random.Next())
                        .Take(serviceCount)
                        .ToList();

                    // Crear statements de INSERT para cada service
                    foreach (var serviceId in selectedServices)
                    {
                        insertStatements.Add($"({coworkingSpaceId}, {serviceId})");
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
                    var sql = $"INSERT INTO CoworkingSpaceServiceOffered (CoworkingSpacesId, ServicesId) VALUES {valuesClause}";

                    await context.Database.ExecuteSqlRawAsync(sql);
                    
                    Console.WriteLine($"   Lote {batch + 1}/{totalBatches} insertado ({batchStatements.Count} registros)");
                }

                Console.WriteLine("🎉 ¡CoworkingSpaceServiceOffered seeding completado exitosamente!");
                
                // Mostrar estadísticas finales
                var finalCount = await context.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(*) as Value FROM CoworkingSpaceServiceOffered").FirstAsync();
                Console.WriteLine($"📊 Total de relaciones creadas: {finalCount}");
                Console.WriteLine($"📊 Promedio de services por CoworkingSpace: {finalCount / 300.0:F1}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en CoworkingSpaceServiceOfferedSeeder: {ex.Message}");
                Console.WriteLine($"🔍 Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
} 