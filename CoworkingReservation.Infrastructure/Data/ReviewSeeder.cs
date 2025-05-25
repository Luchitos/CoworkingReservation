using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoworkingReservation.Infrastructure.Data
{
    public static class ReviewSeeder
    {
        public static async Task SeedReviewsAsync(ApplicationDbContext context)
        {
            // Verificar si ya existen reviews
            var existingReviewsCount = await context.Reviews.CountAsync();
            if (existingReviewsCount > 0)
            {
                Console.WriteLine($"✅ Ya existen {existingReviewsCount} reviews en la base de datos.");
                return;
            }

            Console.WriteLine("⭐ Iniciando creación de reviews basadas en reservas completadas...");

            // Obtener reservas completadas del pasado
            var completedReservations = await context.Reservations
                .Where(r => r.Status == ReservationStatus.Completed && r.EndDate < DateTime.Today)
                .Include(r => r.User)
                .Include(r => r.CoworkingSpace)
                .ToListAsync();

            if (completedReservations.Count == 0)
            {
                Console.WriteLine("❌ No se encontraron reservas completadas para generar reviews.");
                return;
            }

            Console.WriteLine($"📍 Encontradas {completedReservations.Count} reservas completadas para evaluar");

            var random = new Random(42); // Seed fijo para resultados reproducibles
            var allReviews = new List<Review>();

            // Generar reviews para un porcentaje de las reservas completadas
            var reviewProbability = 0.65; // 65% de las reservas completadas tendrán review
            var reviewsToGenerate = (int)(completedReservations.Count * reviewProbability);

            // Seleccionar aleatoriamente qué reservas tendrán review
            var reservationsForReview = completedReservations
                .OrderBy(x => random.Next())
                .Take(reviewsToGenerate)
                .ToList();

            Console.WriteLine($"🎯 Generando reviews para {reservationsForReview.Count} reservas ({reviewProbability * 100:F0}% del total)");

            // Generar reviews
            foreach (var reservation in reservationsForReview)
            {
                var review = CreateReviewForReservation(reservation, random);
                allReviews.Add(review);
            }

            // Insertar reviews en lotes
            await InsertReviewsInBatches(context, allReviews);

            // Mostrar estadísticas
            ShowReviewStatistics(allReviews, completedReservations.Count);
        }

        private static Review CreateReviewForReservation(Reservation reservation, Random random)
        {
            // Generar rating con distribución realista (sesgada hacia valores altos)
            var rating = GenerateRealisticRating(random);
            
            // Generar comentario basado en el rating
            var comment = GenerateCommentBasedOnRating(rating, random);
            
            // Fecha de creación: entre 1-30 días después del fin de la reserva
            var daysAfterReservation = random.Next(1, 31);
            var createdAt = reservation.EndDate.AddDays(daysAfterReservation);
            
            // Asegurar que no sea futuro
            if (createdAt > DateTime.UtcNow)
            {
                createdAt = DateTime.UtcNow.AddDays(-random.Next(1, 15));
            }

            return new Review
            {
                UserId = reservation.UserId,
                CoworkingSpaceId = reservation.CoworkingSpaceId,
                Rating = rating,
                Comment = comment,
                CreatedAt = createdAt
            };
        }

        private static int GenerateRealisticRating(Random random)
        {
            // Distribución realista de ratings (sesgada hacia valores altos)
            var probability = random.NextDouble();
            return probability switch
            {
                < 0.05 => 1,  // 5% - Muy malo
                < 0.15 => 2,  // 10% - Malo
                < 0.30 => 3,  // 15% - Regular
                < 0.60 => 4,  // 30% - Bueno
                _ => 5        // 40% - Excelente
            };
        }

        private static string GenerateCommentBasedOnRating(int rating, Random random)
        {
            var positiveComments = new[]
            {
                "Excelente espacio de trabajo, muy cómodo y bien ubicado.",
                "Perfecto para trabajar concentrado. El ambiente es ideal.",
                "Muy buena atención y el espacio está impecable.",
                "Recomendado al 100%. Volveré sin dudas.",
                "Instalaciones modernas y todo muy limpio.",
                "El mejor coworking de la zona, sin dudas.",
                "Ambiente profesional y muy buena conectividad.",
                "Superó mis expectativas. Muy recomendable.",
                "Lugar ideal para trabajar en equipo.",
                "Excelente relación calidad-precio.",
                "Personal muy amable y servicio de primera.",
                "Espacios amplios y muy bien iluminados.",
                "Todo perfecto, desde la reserva hasta el check-out.",
                "Muy buena experiencia, definitivamente volvería.",
                "Ambiente tranquilo y productivo."
            };

            var neutralComments = new[]
            {
                "Está bien, cumple con lo básico.",
                "Espacio correcto, sin mayores inconvenientes.",
                "Bien ubicado aunque le falta algo de mantenimiento.",
                "Relación calidad-precio aceptable.",
                "Bueno para trabajar ocasionalmente.",
                "Está ok, aunque esperaba un poco más.",
                "Servicio estándar, nada extraordinario.",
                "Cumple su función básica de coworking.",
                "Bien en general, con algunas cosas por mejorar.",
                "Espacio funcional pero sin lujos."
            };

            var negativeComments = new[]
            {
                "El espacio estaba sucio y mal mantenido.",
                "Muy ruidoso, no se puede trabajar concentrado.",
                "La conexión a internet es muy lenta.",
                "Personal poco amable y servicio deficiente.",
                "No coincide con las fotos, muy decepcionante.",
                "Mobiliario viejo y en mal estado.",
                "Temperatura inadecuada, mucho frío/calor.",
                "No lo recomiendo, hay mejores opciones.",
                "Problemas con la reserva y poca flexibilidad.",
                "Muy caro para lo que ofrece.",
                "Falta de limpieza y orden.",
                "Ambiente poco profesional.",
                "Instalaciones deterioradas.",
                "No volvería, mala experiencia.",
                "Definitivamente no vale la pena."
            };

            return rating switch
            {
                5 => positiveComments[random.Next(positiveComments.Length)],
                4 => random.NextDouble() < 0.8 
                    ? positiveComments[random.Next(positiveComments.Length)]
                    : neutralComments[random.Next(neutralComments.Length)],
                3 => neutralComments[random.Next(neutralComments.Length)],
                2 => random.NextDouble() < 0.7
                    ? negativeComments[random.Next(negativeComments.Length)]
                    : neutralComments[random.Next(neutralComments.Length)],
                1 => negativeComments[random.Next(negativeComments.Length)],
                _ => neutralComments[random.Next(neutralComments.Length)]
            };
        }

        private static async Task InsertReviewsInBatches(ApplicationDbContext context, List<Review> allReviews)
        {
            Console.WriteLine("💾 Insertando reviews en la base de datos...");
            
            var batchSize = 100;
            var processedCount = 0;
            
            for (int i = 0; i < allReviews.Count; i += batchSize)
            {
                var batch = allReviews.Skip(i).Take(batchSize);
                await context.Reviews.AddRangeAsync(batch);
                await context.SaveChangesAsync();
                
                processedCount += batch.Count();
                
                if (processedCount % 200 == 0 || processedCount == allReviews.Count)
                {
                    Console.WriteLine($"✅ Procesadas {processedCount}/{allReviews.Count} reviews...");
                }
            }
            
            Console.WriteLine($"🎉 ¡{allReviews.Count} reviews creadas exitosamente!");
        }

        private static void ShowReviewStatistics(List<Review> allReviews, int totalCompletedReservations)
        {
            var ratingStats = allReviews.GroupBy(r => r.Rating).ToDictionary(g => g.Key, g => g.Count());
            var averageRating = allReviews.Average(r => r.Rating);
            var reviewPercentage = (double)allReviews.Count / totalCompletedReservations * 100;

            // Contar reviews por CoworkingSpace
            var reviewsBySpace = allReviews.GroupBy(r => r.CoworkingSpaceId).ToDictionary(g => g.Key, g => g.Count());
            var avgReviewsPerSpace = reviewsBySpace.Values.Average();
            var maxReviewsPerSpace = reviewsBySpace.Values.Max();
            var minReviewsPerSpace = reviewsBySpace.Values.Min();

            Console.WriteLine($"⭐ Estadísticas de reviews generadas:");
            Console.WriteLine($"   📊 Total de reviews: {allReviews.Count}");
            Console.WriteLine($"   📈 Porcentaje de reservas con review: {reviewPercentage:F1}%");
            Console.WriteLine($"   🌟 Rating promedio: {averageRating:F2}/5");
            Console.WriteLine($"   ⭐ Distribución de ratings:");
            for (int i = 1; i <= 5; i++)
            {
                var count = ratingStats.GetValueOrDefault(i, 0);
                var percentage = (double)count / allReviews.Count * 100;
                var stars = new string('★', i);
                Console.WriteLine($"      {stars}: {count} reviews ({percentage:F1}%)");
            }
            Console.WriteLine($"   🏢 Reviews por espacio:");
            Console.WriteLine($"      - Promedio: {avgReviewsPerSpace:F1} reviews");
            Console.WriteLine($"      - Máximo: {maxReviewsPerSpace} reviews");
            Console.WriteLine($"      - Mínimo: {minReviewsPerSpace} reviews");
            Console.WriteLine($"   📅 Espacios con reviews: {reviewsBySpace.Count}/300");
        }
    }
} 