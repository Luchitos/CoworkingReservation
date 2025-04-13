namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    /// <summary>
    /// DTO para representar características especiales de un espacio de coworking.
    /// </summary>
    public class SpecialFeatureDTO
    {
        /// <summary>
        /// Identificador único de la característica especial.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre de la característica especial.
        /// </summary>
        public string Name { get; set; }
    }
} 