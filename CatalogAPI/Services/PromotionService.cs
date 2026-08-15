using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatalogAPI.Data;
using CatalogAPI.DTOs;
using CatalogAPI.Entities;
using MongoDB.Driver;

namespace CatalogAPI.Services;

public class PromotionService : IPromotionService
{
    private readonly CatalogMongoContext _context;

    public PromotionService(CatalogMongoContext context)
    {
        _context = context;
    }

    public async Task<Promotion> CreateAsync(CreatePromotionDto dto)
    {
        var existingPromotion = await _context.Promotions.Find(p => p.Code == dto.Code.ToUpper().Trim()).FirstOrDefaultAsync();
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

        await _context.Promotions.InsertOneAsync(promotion);
        return promotion;
    }

    public async Task<Promotion?> UpdateAsync(Guid id, UpdatePromotionDto dto)
    {
        var update = Builders<Promotion>.Update
            .Set(p => p.Description, dto.Description.Trim())
            .Set(p => p.DiscountPercentage, dto.DiscountPercentage)
            .Set(p => p.StartDate, dto.StartDate)
            .Set(p => p.EndDate, dto.EndDate)
            .Set(p => p.IsActive, dto.IsActive)
            .Set(p => p.MaxUses, dto.MaxUses);

        var options = new FindOneAndUpdateOptions<Promotion> { ReturnDocument = ReturnDocument.After };
        return await _context.Promotions.FindOneAndUpdateAsync(p => p.Id == id, update, options);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await _context.Promotions.DeleteOneAsync(p => p.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<List<Promotion>> GetAllAsync()
    {
        return await _context.Promotions.Find(_ => true).ToListAsync();
    }

    public async Task<Promotion?> GetByIdAsync(Guid id)
    {
        return await _context.Promotions.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Promotion?> GetByCodeAsync(string code)
    {
        return await _context.Promotions.Find(p => p.Code == code.ToUpper().Trim()).FirstOrDefaultAsync();
    }
}
