﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoworkingReservation.Domain.Entities;

namespace CoworkingReservation.Application.Services.Interfaces
{
    public interface ISpecialFeatureService
    {
        Task<IEnumerable<SpecialFeature>> GetAllAsync();
        Task<SpecialFeature?> GetByIdAsync(int id);
        Task<SpecialFeature> CreateAsync(SpecialFeature specialFeature);
        Task DeleteAsync(int id);
    }
}