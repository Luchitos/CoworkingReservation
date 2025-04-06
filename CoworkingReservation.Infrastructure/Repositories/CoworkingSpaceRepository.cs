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
                    cs.Address.City.Contains(location) ||
                    cs.Address.Province.Contains(location) ||
                    cs.Address.Street.Contains(location));
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
            var data = await _context.CoworkingSpaces
                .AsNoTracking()
                .Where(cs => cs.IsActive && cs.Status == CoworkingStatus.Approved)
                .Select(cs => new
                {
                    cs.Id,
                    cs.Name,
                    Address = cs.Address,
                    CoverPhotoUrl = cs.Photos.Where(p => p.IsCoverPhoto).Select(p => p.FilePath).FirstOrDefault()
                })
                .ToListAsync();

            return data.Select(cs => new CoworkingSpaceListItemDTO
            {
                Id = cs.Id,
                Name = cs.Name,
                Address = new AddressDTO
                {
                    City = cs.Address.City,
                    Province = cs.Address.Province,
                    Street = cs.Address.Street,
                    Number = cs.Address.Number,
                    Country = cs.Address.Country,
                    ZipCode = cs.Address.ZipCode
                },
                CoverPhotoUrl = cs.CoverPhotoUrl
            }).ToList();
        }

        public async Task<IEnumerable<CoworkingSpaceListItemDTO>> GetFilteredLightweightAsync(int? capacity, string? location)
        {
            var query = _context.CoworkingSpaces
                .AsNoTracking()
                .Where(cs => cs.IsActive && cs.Status == CoworkingStatus.Approved);

            if (capacity.HasValue)
            {
                query = query.Where(cs => cs.CapacityTotal >= capacity.Value);
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(cs =>
                    cs.Address.City.Contains(location) ||
                    cs.Address.Province.Contains(location) ||
                    cs.Address.Street.Contains(location));
            }

            var data = await query
                .Select(cs => new
                {
                    cs.Id,
                    cs.Name,
                    Address = cs.Address,
                    CoverPhotoUrl = cs.Photos.Where(p => p.IsCoverPhoto).Select(p => p.FilePath).FirstOrDefault(),
                    Rate = cs.Rate,
                })
                .ToListAsync();

            return data.Select(cs => new CoworkingSpaceListItemDTO
            {
                Id = cs.Id,
                Name = cs.Name,
                Address = new AddressDTO
                {
                    City = cs.Address.City,
                    Province = cs.Address.Province,
                    Street = cs.Address.Street,
                    Number = cs.Address.Number,
                    Country = cs.Address.Country,
                    ZipCode = cs.Address.ZipCode,
                    Latitude = cs.Address.Latitude,
                    Longitude = cs.Address.Longitude
                },
                CoverPhotoUrl = cs.CoverPhotoUrl,
                Rate = cs.Rate,
            });
        }
    }
}
