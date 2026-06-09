using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatalogAPI.Data;
using CatalogAPI.DTOs;
using CatalogAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Services;

public class PromotionService : IPromotionService
{
    private readonly CatalogDbContext _context;

    public PromotionService(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Promotion> CreateAsync(CreatePromotionDto dto)
    {
        var existingPromotion = await _context.Promotions.FirstOrDefaultAsync(p => p.Code == dto.Code.ToUpper().Trim());
        if (existingPromotion != null)
            throw new InvalidOperationException("Já existe uma promoção com este código.");

        var promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            Code = dto.Code.ToUpper().Trim(),
            Description = dto.Description.Trim(),
            DiscountPercentage = dto.DiscountPercentage,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = true,
            MaxUses = dto.MaxUses,
            CurrentUses = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync();
        return promotion;
    }

    public async Task<Promotion?> UpdateAsync(Guid id, UpdatePromotionDto dto)
    {
        var promotion = await _context.Promotions.FindAsync(id);
        if (promotion == null)
            return null;

        promotion.Description = dto.Description.Trim();
        promotion.DiscountPercentage = dto.DiscountPercentage;
        promotion.StartDate = dto.StartDate;
        promotion.EndDate = dto.EndDate;
        promotion.IsActive = dto.IsActive;
        promotion.MaxUses = dto.MaxUses;

        await _context.SaveChangesAsync();
        return promotion;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var promotion = await _context.Promotions.FindAsync(id);
        if (promotion == null)
            return false;

        _context.Promotions.Remove(promotion);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<Promotion>> GetAllAsync()
    {
        return await _context.Promotions.ToListAsync();
    }

    public async Task<Promotion?> GetByIdAsync(Guid id)
    {
        return await _context.Promotions.FindAsync(id);
    }

    public async Task<Promotion?> GetByCodeAsync(string code)
    {
        return await _context.Promotions.FirstOrDefaultAsync(p => p.Code == code.ToUpper().Trim());
    }
}
