using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Enums;

namespace CoworkingReservation.Application.DTOs.CoworkingSpace
{
    public class CoworkingSpaceResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerDay { get; set; }
        public bool IsActive { get; set; }
        public AddressDTO Address { get; set; }
        public List<PhotoResponseDTO> Photos { get; set; }
        public List<ServiceOfferedDTO> Services { get; set; }
        public List<BenefitDTO> Benefits { get; set; }
        public List<CoworkingAreaResponseDTO> Areas { get; set; }

    }
    public class PhotoResponseDTO
    {
        public string FileName { get; set; }
        public bool IsCoverPhoto { get; set; }
        public string ContentType { get; set; }
        public string FilePath { get; set; }
    }
    public class ServiceOfferedDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class BenefitDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class CoworkingAreaResponseDTO
    {
        public int Id { get; set; }
        public CoworkingAreaType Type { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerDay { get; set; }
    }
}