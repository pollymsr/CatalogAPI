using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatalogAPI.DTOs;
using CatalogAPI.Entities;

namespace CatalogAPI.Services;

public interface IPromotionService
{
    Task<Promotion> CreateAsync(CreatePromotionDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<List<Promotion>> GetAllAsync();
    Task<Promotion?> GetByIdAsync(Guid id);
    Task<Promotion?> GetByCodeAsync(string code);
    Task<Promotion?> UpdateAsync(Guid id, UpdatePromotionDto dto);
}
