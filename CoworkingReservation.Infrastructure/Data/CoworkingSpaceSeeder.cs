using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CoworkingReservation.Infrastructure.Data
{
    public static class CoworkingSpaceSeeder
    {
        public static async Task SeedCoworkingSpacesAsync(ApplicationDbContext context)
        {
            // Verificar si ya existen CoworkingSpaces para los 300 usuarios
            var existingCount = await context.CoworkingSpaces.CountAsync(cs => cs.HosterId >= 1 && cs.HosterId <= 300);
            if (existingCount >= 300)
            {
                Console.WriteLine("✅ Los 300 CoworkingSpaces ya existen en la base de datos.");
                return;
            }

            // Si hay datos parciales, limpiar para empezar de nuevo
            if (existingCount > 0)
            {
                await CleanExistingDataAsync(context);
            }

            Console.WriteLine($"🏢 Iniciando creación de 300 CoworkingSpaces básicos...");

            var random = new Random(42); // Seed fijo para resultados reproducibles
            var creationDate = DateTime.UtcNow;

            // Generar los 300 CoworkingSpaces básicos (uno por cada usuario hoster)
            for (int i = 1; i <= 300; i++)
            {
                await CreateBasicCoworkingSpaceAsync(context, i, random);
                
                // Mostrar progreso cada 50 espacios
                if (i % 50 == 0)
                {
                    Console.WriteLine($"✅ Creados {i}/300 CoworkingSpaces básicos...");
                }
            }

            Console.WriteLine("🎉 ¡300 CoworkingSpaces básicos creados exitosamente!");
        }

        private static async Task CreateBasicCoworkingSpaceAsync(ApplicationDbContext context, int hosterId, Random random)
        {
            // 1. Crear Address
            var address = CreateAddress(hosterId, random);
            context.Addresses.Add(address);
            await context.SaveChangesAsync(); // Guardar para obtener AddressId

            // 2. Crear CoworkingSpace básico
            var coworkingSpace = CreateBasicCoworkingSpace(hosterId, random);
            coworkingSpace.Address = address; // Usar navigation property
            context.CoworkingSpaces.Add(coworkingSpace);
            await context.SaveChangesAsync(); // Guardar CoworkingSpace
        }

        private static Address CreateAddress(int spaceNumber, Random random)
        {
            // Diccionario con coordenadas reales de ciudades argentinas
            var argentineCities = new Dictionary<string, (string Province, double Lat, double Lng)>
            {
                { "Buenos Aires", ("CABA", -34.6118, -58.3960) },
                { "Córdoba", ("Córdoba", -31.4201, -64.1888) },
                { "Rosario", ("Santa Fe", -32.9442, -60.6505) },
                { "Mendoza", ("Mendoza", -32.8908, -68.8272) },
                { "La Plata", ("Buenos Aires", -34.9215, -57.9545) },
                { "San Miguel de Tucumán", ("Tucumán", -26.8083, -65.2176) },
                { "Mar del Plata", ("Buenos Aires", -38.0055, -57.5426) },
                { "Salta", ("Salta", -24.7821, -65.4232) },
                { "Santa Fe", ("Santa Fe", -31.6333, -60.7000) },
                { "San Juan", ("San Juan", -31.5375, -68.5364) },
                { "Neuquén", ("Neuquén", -38.9516, -68.0591) },
                { "Bahía Blanca", ("Buenos Aires", -38.7183, -62.2663) },
                { "Corrientes", ("Corrientes", -27.4806, -58.8341) },
                { "Posadas", ("Misiones", -27.3621, -55.8969) },
                { "San Salvador de Jujuy", ("Jujuy", -24.1858, -65.2995) },
                { "Paraná", ("Entre Ríos", -31.7319, -60.5297) },
                { "Formosa", ("Formosa", -26.1775, -58.1781) },
                { "San Luis", ("San Luis", -33.2950, -66.3356) },
                { "Catamarca", ("Catamarca", -28.4696, -65.7852) },
                { "La Rioja", ("La Rioja", -29.4130, -66.8506) }
            };

            // Ciudades de otros países (para variedad)
            var otherCities = new Dictionary<string, (string Province, string Country, double Lat, double Lng)>
            {
                { "Santiago", ("Región Metropolitana", "Chile", -33.4489, -70.6693) },
                { "Montevideo", ("Montevideo", "Uruguay", -34.9011, -56.1645) },
                { "Asunción", ("Asunción", "Paraguay", -25.2637, -57.5759) },
                { "La Paz", ("La Paz", "Bolivia", -16.5000, -68.1193) }
            };

            var streets = new[] { 
                "Av. Corrientes", "Av. Santa Fe", "Av. Rivadavia", "Av. Cabildo", 
                "Florida", "Lavalle", "San Martín", "Maipú", "Av. 9 de Julio",
                "Av. Las Heras", "Defensa", "Reconquista", "25 de Mayo", "Belgrano",
                "Sarmiento", "Mitre", "Moreno", "Alsina", "Av. Callao", "Av. Pueyrredón"
            };

            // Seleccionar ciudad (80% Argentina, 20% otros países)
            string selectedCity;
            string province;
            string country;
            double baseLat, baseLng;

            if (random.NextDouble() < 0.8) // 80% ciudades argentinas
            {
                var cityKeys = argentineCities.Keys.ToArray();
                selectedCity = cityKeys[random.Next(cityKeys.Length)];
                var cityData = argentineCities[selectedCity];
                province = cityData.Province;
                country = "Argentina";
                baseLat = cityData.Lat;
                baseLng = cityData.Lng;
            }
            else // 20% otras ciudades
            {
                var cityKeys = otherCities.Keys.ToArray();
                selectedCity = cityKeys[random.Next(cityKeys.Length)];
                var cityData = otherCities[selectedCity];
                province = cityData.Province;
                country = cityData.Country;
                baseLat = cityData.Lat;
                baseLng = cityData.Lng;
            }

            // Agregar variación pequeña para simular diferentes ubicaciones dentro de la ciudad
            // ±0.01 grados ≈ ±1.1 km de variación
            var latVariation = (random.NextDouble() - 0.5) * 0.02; // ±0.01 grados
            var lngVariation = (random.NextDouble() - 0.5) * 0.02; // ±0.01 grados

            var finalLat = baseLat + latVariation;
            var finalLng = baseLng + lngVariation;

            return new Address
            {
                City = selectedCity,
                Country = country,
                Province = province,
                Street = streets[random.Next(streets.Length)],
                Number = random.Next(100, 9999).ToString(),
                StreetOne = streets[random.Next(streets.Length)],
                StreetTwo = $"Piso {random.Next(1, 15)}",
                ZipCode = random.Next(1000, 9999).ToString(),
                Latitude = finalLat.ToString("F6", CultureInfo.InvariantCulture),
                Longitude = finalLng.ToString("F6", CultureInfo.InvariantCulture)
            };
        }

        private static CoworkingSpace CreateBasicCoworkingSpace(int hosterId, Random random)
        {
            var names = new[] { 
                "Espacio Creativo", "Hub de Innovación", "Centro Colaborativo", "Oficina Moderna", 
                "Loft Urbano", "Cowork Central", "Nexus Workspace", "Impact Hub", 
                "La Colmena", "Digital Nomads", "WorkLab", "Space Pro", 
                "CoLab", "The Hive", "WorkPoint", "Office Space", 
                "Creative Hub", "Business Center", "Work Zone", "Smart Office"
            };
            
            var descriptions = new[] {
                "Un moderno espacio de coworking diseñado para emprendedores y freelancers que buscan un ambiente colaborativo y productivo.",
                "Espacio innovador que fomenta la creatividad y el networking entre profesionales de diferentes industrias.",
                "Ambiente profesional equipado con la última tecnología para equipos remotos y startups en crecimiento.",
                "Centro de coworking que combina diseño contemporáneo con funcionalidad para maximizar la productividad.",
                "Un espacio flexible y dinámico perfecto para profesionales independientes y pequeños equipos.",
                "Oficina compartida con todas las comodidades necesarias para trabajar de manera eficiente y cómoda.",
                "Espacio de trabajo colaborativo que promueve la innovación y el intercambio de ideas entre profesionales.",
                "Centro empresarial moderno con espacios versátiles para diferentes tipos de trabajo y reuniones."
            };

            return new CoworkingSpace
            {
                Name = $"{names[random.Next(names.Length)]} {hosterId}",
                Description = descriptions[random.Next(descriptions.Length)],
                CapacityTotal = random.Next(10, 100), // Capacidad entre 10 y 100 personas
                IsActive = true,
                Rate = (float)Math.Round(random.NextDouble() * 4 + 1, 1), // Rate entre 1.0 y 5.0
                Status = CoworkingStatus.Approved, // Todos empiezan como pendientes
                HosterId = hosterId // Asignar al usuario correspondiente (1-300)
            };
        }

        private static async Task CleanExistingDataAsync(ApplicationDbContext context)
        {
            Console.WriteLine("🧹 Limpiando datos parciales existentes...");

            // Obtener CoworkingSpaces existentes con sus relaciones
            var existingSpaces = await context.CoworkingSpaces
                .Include(cs => cs.Address)
                .Where(cs => cs.HosterId >= 1 && cs.HosterId <= 300)
                .ToListAsync();

            foreach (var space in existingSpaces)
            {
                // Eliminar CoworkingSpace (las relaciones en cascada se eliminarán automáticamente)
                context.CoworkingSpaces.Remove(space);
                
                // Eliminar Address asociada
                if (space.Address != null)
                {
                    context.Addresses.Remove(space.Address);
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine("✅ Datos parciales limpiados correctamente");
        }
    }
} 