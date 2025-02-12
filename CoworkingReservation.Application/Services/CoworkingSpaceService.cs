using CoworkingReservation.Application.DTOs.CoworkingSpace;
using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.IRepository;

namespace CoworkingReservation.Application.Services
{
    public class CoworkingSpaceService : ICoworkingSpaceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CoworkingSpaceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CoworkingSpace> CreateAsync(CreateCoworkingSpaceDTO spaceDto, int hosterId)
        {
            var hoster = await _unitOfWork.Users.GetByIdAsync(hosterId);
            if (hoster == null || hoster.Role != "Hoster")
                throw new UnauthorizedAccessException("Only hosters can create coworking spaces");

            var coworkingSpace = new CoworkingSpace
            {
                Name = spaceDto.Name,
                Description = spaceDto.Description,
                Capacity = spaceDto.Capacity,
                PricePerDay = spaceDto.PricePerDay,
                Address = new Address
                {
                    City = spaceDto.Address.City,
                    Country = spaceDto.Address.Country,
                    Apartment = spaceDto.Address.Apartment,
                    Floor = spaceDto.Address.Floor,
                    Number = spaceDto.Address.Number,
                    Province = spaceDto.Address.Province,
                    Street = spaceDto.Address.Street,
                    ZipCode = spaceDto.Address.ZipCode
                },
                Photos = new List<Photo>()
            };

            //spaceDto.Photos = new List<Photo>();
            if (spaceDto.Photos != null && spaceDto.Photos.Count > 0)
            {
                string[] allowedMimeTypes = { "image/jpeg", "image/png", "image/jpg" };

                foreach (var photo in spaceDto.Photos)
                {
                    if (!allowedMimeTypes.Contains(photo.ContentType.ToLower()))
                    {
                        throw new ArgumentException($"Invalid file format: {photo.ContentType}");
                    }

                    using var memoryStream = new MemoryStream();
                    await photo.CopyToAsync(memoryStream);

                    coworkingSpace.Photos.Add(new Photo
                    {
                        FileName = photo.FileName,
                        FotoData = memoryStream.ToArray(),
                        ContentType = photo.ContentType,
                        IsCoverPhoto = false
                    });
                }

                var firstPhoto = coworkingSpace.Photos.FirstOrDefault();
                if (firstPhoto != null)
                {
                    firstPhoto.IsCoverPhoto = true;
                }
            }

            await _unitOfWork.CoworkingSpaces.AddAsync(coworkingSpace);
            await _unitOfWork.SaveChangesAsync();

            return coworkingSpace;
        }

        public async Task DeleteAsync(int id, int hosterId)
        {
            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(id);
            if (coworkingSpace == null) throw new KeyNotFoundException("Coworking space not found");

            await _unitOfWork.CoworkingSpaces.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllActiveSpacesAsync()
        {
            var spaces = await _unitOfWork.CoworkingSpaces
                .GetAllAsync(includeProperties: "Address,Photos");

            return spaces
                .Where(cs => cs.IsActive)
                .Select(cs => new CoworkingSpaceResponseDTO
                {
                    Id = cs.Id,
                    Name = cs.Name,
                    Description = cs.Description,
                    Capacity = cs.Capacity,
                    PricePerDay = cs.PricePerDay,
                    IsActive = cs.IsActive,
                    Address = cs.Address != null ? new AddressDTO
                    {
                        City = cs.Address.City,
                        Country = cs.Address.Country,
                        Number = cs.Address.Number,
                        Province = cs.Address.Province,
                        Street = cs.Address.Street,
                        ZipCode = cs.Address.ZipCode
                    } : null,
                    Photos = cs.Photos?.Select(p => new PhotoResponseDTO
                    {
                        FileName = p.FileName,
                        IsCoverPhoto = p.IsCoverPhoto,
                        ContentType = p.ContentType
                    }).ToList() ?? new List<PhotoResponseDTO>()
                })
                .ToList();
        }

        public async Task<CoworkingSpaceResponseDTO> GetByIdAsync(int id)
        {
            var cs = await _unitOfWork.CoworkingSpaces
                .GetByIdAsync(id, includeProperties: "Address,Photos");

            if (cs == null) throw new KeyNotFoundException("Coworking space not found");

            return new CoworkingSpaceResponseDTO
            {
                Id = cs.Id,
                Name = cs.Name,
                Description = cs.Description,
                Capacity = cs.Capacity,
                PricePerDay = cs.PricePerDay,
                IsActive = cs.IsActive,
                Address = cs.Address != null ? new AddressDTO
                {
                    City = cs.Address.City,
                    Country = cs.Address.Country,
                    Number = cs.Address.Number,
                    Province = cs.Address.Province,
                    Street = cs.Address.Street,
                    ZipCode = cs.Address.ZipCode
                } : null,
                Photos = cs.Photos?.Select(p => new PhotoResponseDTO
                {
                    FileName = p.FileName,
                    IsCoverPhoto = p.IsCoverPhoto,
                    ContentType = p.ContentType
                }).ToList() ?? new List<PhotoResponseDTO>()
            };
        }

        public async Task UpdateAsync(int id, CreateCoworkingSpaceDTO dto, int hosterId)
        {
            var coworkingSpace = await _unitOfWork.CoworkingSpaces.GetByIdAsync(id);
            if (coworkingSpace == null)
                throw new KeyNotFoundException("Coworking space not found");

            if (coworkingSpace.Id != hosterId)
                throw new UnauthorizedAccessException("You can only update your own coworking spaces.");

            // Actualizar propiedades básicas
            coworkingSpace.Name = dto.Name;
            coworkingSpace.Description = dto.Description;
            coworkingSpace.PricePerDay = dto.PricePerDay;
            coworkingSpace.Capacity = dto.Capacity;

            // Actualizar dirección
            coworkingSpace.Address.City = dto.Address.City;
            coworkingSpace.Address.Country = dto.Address.Country;
            coworkingSpace.Address.Apartment = dto.Address.Apartment;
            coworkingSpace.Address.Floor = dto.Address.Floor;
            coworkingSpace.Address.Number = dto.Address.Number;
            coworkingSpace.Address.Province = dto.Address.Province;
            coworkingSpace.Address.Street = dto.Address.Street;
            coworkingSpace.Address.ZipCode = dto.Address.ZipCode;

            // Manejo de fotos si se proporcionan nuevas
            if (dto.Photos != null && dto.Photos.Count > 0)
            {
                coworkingSpace.Photos.Clear(); // Eliminar fotos antiguas

                foreach (var photo in dto.Photos)
                {
                    using var memoryStream = new MemoryStream();
                    await photo.CopyToAsync(memoryStream);

                    var newPhoto = new Photo
                    {
                        FileName = photo.FileName,
                        FotoData = memoryStream.ToArray(),
                        ContentType = photo.ContentType,
                        IsCoverPhoto = false
                    };

                    coworkingSpace.Photos.Add(newPhoto);
                }

                // Marcar la primera foto como portada sin indexar directamente
                var firstPhoto = coworkingSpace.Photos.FirstOrDefault();
                if (firstPhoto != null)
                {
                    firstPhoto.IsCoverPhoto = true;
                }
            }

            await _unitOfWork.CoworkingSpaces.UpdateAsync(coworkingSpace);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
