using CoworkingReservation.Domain.DTOs;
using CoworkingReservation.Domain.Entities;
using CoworkingReservation.Domain.Enums;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;
using CoworkingReservation.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CoworkingReservation.Infrastructure.Repositories
{
    public class CoworkingSpaceRepository : Repository<CoworkingSpace>, ICoworkingSpaceRepository
    {
        public CoworkingSpaceRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<CoworkingSpace>> GetActiveSpacesAsync()
        {
            return await _dbSet.Where(cs => cs.IsActive).ToListAsync();
        }

        public IQueryable<CoworkingSpace> GetQueryable(string includeProperties = "")
        {
            IQueryable<CoworkingSpace> query = _dbSet.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            return query;
        }
        
        public async Task<IEnumerable<CoworkingSpaceResponseDTO>> GetAllFilteredAsync(int? capacity, string? location)
        {
            var query = _dbSet
                .Include(cs => cs.Address)
                .Include(cs => cs.Photos)
                .Where(cs => cs.Status == CoworkingStatus.Approved && cs.IsActive);

            if (capacity.HasValue)
            {
                query = query.Where(cs => cs.CapacityTotal >= capacity.Value);
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(cs =>
                    cs.Address != null && (
                    cs.Address.City.Contains(location) ||
                    cs.Address.Province.Contains(location) ||
                    cs.Address.Street.Contains(location)));
            }

            var spaces = await query.ToListAsync();

            return spaces.Select(cs => new CoworkingSpaceResponseDTO
            {
                Id = cs.Id,
                Name = cs.Name,
                Description = cs.Description,
                CapacityTotal = cs.CapacityTotal,
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
                    ContentType = p.MimeType
                }).ToList() ?? new List<PhotoResponseDTO>()
            }).ToList();
        }

        public IQueryable<CoworkingSpace> GetFilteredQuery()
        {
            return _dbSet
                .Include(cs => cs.Address)
                .Include(cs => cs.Photos)
                .Where(cs => cs.Status == CoworkingStatus.Approved && cs.IsActive);
        }

        public async Task<bool> ExistsAsync(Expression<Func<CoworkingSpace, bool>> predicate)
        {
            return await _context.CoworkingSpaces.AnyAsync(predicate);
        }

        public async Task<List<CoworkingSpaceListItemDTO>> GetAllLightweightAsync()
        {
            var query = _context.CoworkingSpaces
                .AsNoTracking()
                .Include(cs => cs.Address)
                .Include(cs => cs.Areas)
                .Include(cs => cs.Photos)
                .Where(cs => cs.IsActive && cs.Status == CoworkingStatus.Approved);
                
            // Log the SQL query
            Console.WriteLine("DEBUG SQL QUERY: " + query.ToQueryString());
            
            var rawSpaces = await query.ToListAsync();
            
            // Depuración para ver si hay áreas
            Console.WriteLine($"DEBUG: GetAllLightweightAsync - Found {rawSpaces.Count} spaces");
            foreach (var space in rawSpaces)
            {
                // Forzar inicialización de colección de áreas si es null
                if (space.Areas == null)
                {
                    space.Areas = new List<CoworkingArea>();
                }
                
                bool hasAreas = space.Areas.Any();
                Console.WriteLine($"DEBUG: Space {space.Id} '{space.Name}' has {space.Areas.Count} areas, hasAreas={hasAreas}");
                
                if (hasAreas)
                {
                    foreach (var area in space.Areas)
                    {
                        Console.WriteLine($"DEBUG: - Area {area.Id}, Type: {area.Type}, Price: {area.PricePerDay}, Available: {area.Available}");
                    }
                }
            }
            
            var data = rawSpaces.Select(cs => new
            {
                cs.Id,
                cs.Name,
                Address = cs.Address,
                CoverPhotoUrl = cs.Photos.Where(p => p.IsCoverPhoto).Select(p => p.FilePath).FirstOrDefault(),
                Rate = cs.Rate,
                Areas = cs.Areas.Select(a => new { a.Type, a.Capacity, a.PricePerDay, a.Available }).ToList(),
                TotalCapacity = cs.CapacityTotal,
                HasAreas = cs.Areas.Any()
            }).ToList();

            return data.Select(cs => new CoworkingSpaceListItemDTO
            {
                Id = cs.Id,
                Name = cs.Name,
                Address = cs.Address != null ? new AddressDTO
                {
                    City = cs.Address.City,
                    Province = cs.Address.Province,
                    Street = cs.Address.Street,
                    Number = cs.Address.Number,
                    Country = cs.Address.Country,
                    ZipCode = cs.Address.ZipCode,
                    Latitude = cs.Address?.Latitude,
                    Longitude = cs.Address?.Longitude
                } : null,
                CoverPhotoUrl = cs.CoverPhotoUrl,
                Rate = cs.Rate,
                TotalCapacity = cs.TotalCapacity,
                HasConfiguredAreas = cs.HasAreas,
                PrivateOfficesCount = cs.HasAreas ? cs.Areas.Count(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available) : 0,
                IndividualDesksCount = cs.HasAreas ? cs.Areas.Count(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available) : 0,
                SharedDesksCount = cs.HasAreas ? cs.Areas.Count(a => a.Type == CoworkingAreaType.SharedDesks && a.Available) : 0,
                
                MinPrivateOfficePrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available) 
                    ? cs.Areas.Where(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available).Min(a => a.PricePerDay) 
                    : null,
                MaxPrivateOfficePrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available) 
                    ? cs.Areas.Where(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available).Max(a => a.PricePerDay) 
                    : null,
                
                MinIndividualDeskPrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available) 
                    ? cs.Areas.Where(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available).Min(a => a.PricePerDay) 
                    : null,
                MaxIndividualDeskPrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available) 
                    ? cs.Areas.Where(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available).Max(a => a.PricePerDay) 
                    : null,
                
                SharedDeskPrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.SharedDesks && a.Available) 
                    ? cs.Areas.Where(a => a.Type == CoworkingAreaType.SharedDesks && a.Available).Min(a => a.PricePerDay) 
                    : null
            }).ToList();
        }

        public async Task<IEnumerable<CoworkingSpaceListItemDTO>> GetFilteredLightweightAsync(int? capacity, string? location)
        {
            var query = _context.CoworkingSpaces
                .AsNoTracking()
                .Include(cs => cs.Address)
                .Include(cs => cs.Areas)
                .Include(cs => cs.Photos)
                .Where(cs => cs.IsActive && cs.Status == CoworkingStatus.Approved);

            if (capacity.HasValue)
            {
                query = query.Where(cs => cs.CapacityTotal >= capacity.Value);
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(cs =>
                    cs.Address != null && (
                    cs.Address.City.Contains(location) ||
                    cs.Address.Province.Contains(location) ||
                    cs.Address.Street.Contains(location)));
            }

            // Log the SQL query
            Console.WriteLine("DEBUG FILTERED SQL QUERY: " + query.ToQueryString());
            
            var rawSpaces = await query.ToListAsync();
            
            // Depuración para ver si hay áreas
            Console.WriteLine($"DEBUG: GetFilteredLightweightAsync - Found {rawSpaces.Count} spaces");
            foreach (var space in rawSpaces)
            {
                // Forzar inicialización de colección de áreas si es null
                if (space.Areas == null)
                {
                    space.Areas = new List<CoworkingArea>();
                }
                
                bool hasAreas = space.Areas.Any();
                Console.WriteLine($"DEBUG: Space {space.Id} '{space.Name}' has {space.Areas.Count} areas, hasAreas={hasAreas}");
                
                if (hasAreas)
                {
                    foreach (var area in space.Areas)
                    {
                        Console.WriteLine($"DEBUG: - Area {area.Id}, Type: {area.Type}, Price: {area.PricePerDay}, Available: {area.Available}");
                    }
                }
            }

            var data = rawSpaces.Select(cs => new
            {
                cs.Id,
                cs.Name,
                Address = cs.Address,
                CoverPhotoUrl = cs.Photos.Where(p => p.IsCoverPhoto).Select(p => p.FilePath).FirstOrDefault(),
                Rate = cs.Rate,
                Areas = cs.Areas.Select(a => new { a.Type, a.Capacity, a.PricePerDay, a.Available }).ToList(),
                TotalCapacity = cs.CapacityTotal,
                HasAreas = cs.Areas.Any()
            }).ToList();

            return data.Select(cs => new CoworkingSpaceListItemDTO
            {
                Id = cs.Id,
                Name = cs.Name,
                Address = cs.Address != null ? new AddressDTO
                {
                    City = cs.Address.City,
                    Province = cs.Address.Province,
                    Street = cs.Address.Street,
                    Number = cs.Address.Number,
                    Country = cs.Address.Country,
                    ZipCode = cs.Address.ZipCode,
                    Latitude = cs.Address?.Latitude,
                    Longitude = cs.Address?.Longitude
                } : null,
                CoverPhotoUrl = cs.CoverPhotoUrl,
                Rate = cs.Rate,
                TotalCapacity = cs.TotalCapacity,
                HasConfiguredAreas = cs.HasAreas,
                PrivateOfficesCount = cs.HasAreas ? cs.Areas.Count(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available) : 0,
                IndividualDesksCount = cs.HasAreas ? cs.Areas.Count(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available) : 0,
                SharedDesksCount = cs.HasAreas ? cs.Areas.Count(a => a.Type == CoworkingAreaType.SharedDesks && a.Available) : 0,
                
                MinPrivateOfficePrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available) 
                    ? cs.Areas.Where(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available).Min(a => a.PricePerDay) 
                    : null,
                MaxPrivateOfficePrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available) 
                    ? cs.Areas.Where(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available).Max(a => a.PricePerDay) 
                    : null,
                
                MinIndividualDeskPrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available) 
                    ? cs.Areas.Where(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available).Min(a => a.PricePerDay) 
                    : null,
                MaxIndividualDeskPrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available) 
                    ? cs.Areas.Where(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available).Max(a => a.PricePerDay) 
                    : null,
                
                SharedDeskPrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.SharedDesks && a.Available) 
                    ? cs.Areas.Where(a => a.Type == CoworkingAreaType.SharedDesks && a.Available).Min(a => a.PricePerDay) 
                    : null
            });
        }

        public async Task<IEnumerable<CoworkingSpaceListItemDTO>> GetAllLightweightByIdsAsync(IEnumerable<int> ids)
        {
            var query = _context.CoworkingSpaces
                .AsNoTracking()
                .Include(cs => cs.Address)
                .Include(cs => cs.Areas)
                .Include(cs => cs.Photos)
                .Where(cs => ids.Contains(cs.Id) && cs.IsActive && cs.Status == CoworkingStatus.Approved);

            var rawSpaces = await query.ToListAsync();

            var data = rawSpaces.Select(cs => new
            {
                cs.Id,
                cs.Name,
                Address = cs.Address,
                CoverPhotoUrl = cs.Photos.Where(p => p.IsCoverPhoto).Select(p => p.FilePath).FirstOrDefault(),
                Rate = cs.Rate,
                Areas = cs.Areas.Select(a => new { a.Type, a.Capacity, a.PricePerDay, a.Available }).ToList(),
                TotalCapacity = cs.CapacityTotal,
                HasAreas = cs.Areas.Any()
            }).ToList();

            return data.Select(cs => new CoworkingSpaceListItemDTO
            {
                Id = cs.Id,
                Name = cs.Name,
                Address = cs.Address != null ? new AddressDTO
                {
                    City = cs.Address.City,
                    Province = cs.Address.Province,
                    Street = cs.Address.Street,
                    Number = cs.Address.Number,
                    Country = cs.Address.Country,
                    ZipCode = cs.Address.ZipCode,
                    Latitude = cs.Address?.Latitude,
                    Longitude = cs.Address?.Longitude
                } : null,
                CoverPhotoUrl = cs.CoverPhotoUrl,
                Rate = cs.Rate,
                TotalCapacity = cs.TotalCapacity,
                HasConfiguredAreas = cs.HasAreas,
                PrivateOfficesCount = cs.HasAreas ? cs.Areas.Count(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available) : 0,
                IndividualDesksCount = cs.HasAreas ? cs.Areas.Count(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available) : 0,
                SharedDesksCount = cs.HasAreas ? cs.Areas.Count(a => a.Type == CoworkingAreaType.SharedDesks && a.Available) : 0,

                MinPrivateOfficePrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available)
                    ? cs.Areas.Where(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available).Min(a => a.PricePerDay)
                    : null,
                MaxPrivateOfficePrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available)
                    ? cs.Areas.Where(a => a.Type == CoworkingAreaType.PrivateOffice && a.Available).Max(a => a.PricePerDay)
                    : null,

                MinIndividualDeskPrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available)
                    ? cs.Areas.Where(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available).Min(a => a.PricePerDay)
                    : null,
                MaxIndividualDeskPrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available)
                    ? cs.Areas.Where(a => a.Type == CoworkingAreaType.IndividualDesk && a.Available).Max(a => a.PricePerDay)
                    : null,

                SharedDeskPrice = cs.HasAreas && cs.Areas.Any(a => a.Type == CoworkingAreaType.SharedDesks && a.Available)
                    ? cs.Areas.Where(a => a.Type == CoworkingAreaType.SharedDesks && a.Available).Min(a => a.PricePerDay)
                    : null
            });
        }

    }
}
