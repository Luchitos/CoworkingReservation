using CoworkingReservation.Application.DTOs.Address;

public class CoworkingSpaceListItemDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public AddressDTO Address { get; set; }
    public string? CoverPhotoUrl { get; set; }
    public float Rate { get; set; }

    // Flag para saber si tiene áreas configuradas
    public bool HasConfiguredAreas { get; set; }

    // Información resumida sobre áreas
    public int TotalCapacity { get; set; }
    public int PrivateOfficesCount { get; set; }
    public int IndividualDesksCount { get; set; }
    public int SharedDesksCount { get; set; }

    // Información de precios por tipo
    public decimal? MinPrivateOfficePrice { get; set; }
    public decimal? MaxPrivateOfficePrice { get; set; }
    public decimal? MinIndividualDeskPrice { get; set; }
    public decimal? MaxIndividualDeskPrice { get; set; }
    public decimal? SharedDeskPrice { get; set; }

    // Indica si el espacio está marcado como favorito por el usuario actual
    public bool IsFavorite { get; set; }
}