using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Application.DTOs.User
{
    /// <summary>
    /// DTO para los detalles del usuario en el endpoint /me
    /// </summary>
    public class UserDetailsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public string UserName { get; set; }
        public string Cuit { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreationDate { get; set; }
        public UserPhoto? Photo { get; set; }
    }
} 