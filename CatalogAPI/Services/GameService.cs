using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using CatalogAPI.Data;
using CatalogAPI.DTOs;
using CatalogAPI.Entities;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;

namespace CatalogAPI.Services;

public class GameService : IGameService
{
    private readonly CatalogMongoContext _context;
    private readonly IDistributedCache _cache;

    public GameService(CatalogMongoContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Game> CreateAsync(CreateGameDto dto)
    {
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Price = dto.Price,
            Genre = dto.Genre.Trim(),
            ReleaseDate = dto.ReleaseDate
        };

        await _context.Games.InsertOneAsync(game);
        
        // Invalidate cache
        await _cache.RemoveAsync("games_library");
        
        return game;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await _context.Games.DeleteOneAsync(g => g.Id == id);
        if (result.DeletedCount > 0)
        {
            await _cache.RemoveAsync("games_library");
            return true;
        }
        return false;
    }

    public async Task<List<Game>> GetAllAsync()
    {
        var cachedGames = await _cache.GetStringAsync("games_library");
        if (!string.IsNullOrEmpty(cachedGames))
        {
            return JsonSerializer.Deserialize<List<Game>>(cachedGames) ?? new List<Game>();
        }

        var games = await _context.Games.Find(_ => true).ToListAsync();

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        await _cache.SetStringAsync("games_library", JsonSerializer.Serialize(games), cacheOptions);

        return games;
    }

    public async Task<Game?> GetByIdAsync(Guid id)
    {
        return await _context.Games.Find(g => g.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Game>> GetLibraryAsync(Guid userId)
    {
        var userGames = await _context.UserGames.Find(ug => ug.UserId == userId).ToListAsync();
        var gameIds = userGames.ConvertAll(ug => ug.GameId);
        
        return await _context.Games.Find(g => gameIds.Contains(g.Id)).ToListAsync();
    }

    public async Task<Game?> UpdateAsync(Guid id, UpdateGameDto dto)
    {
        var update = Builders<Game>.Update
            .Set(g => g.Title, dto.Title.Trim())
            .Set(g => g.Description, dto.Description.Trim())
            .Set(g => g.Price, dto.Price)
            .Set(g => g.Genre, dto.Genre.Trim())
            .Set(g => g.ReleaseDate, dto.ReleaseDate);

        var options = new FindOneAndUpdateOptions<Game> { ReturnDocument = ReturnDocument.After };
        
        var game = await _context.Games.FindOneAndUpdateAsync(g => g.Id == id, update, options);
        
        if (game != null)
        {
            await _cache.RemoveAsync("games_library");
        }
        
        return game;
    }
}
