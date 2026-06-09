using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatalogAPI.Data;
using CatalogAPI.DTOs;
using CatalogAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Services;

public class GameService : IGameService
{
    private readonly CatalogDbContext _context;

    public GameService(CatalogDbContext context)
    {
        _context = context;
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

        _context.Games.Add(game);
        await _context.SaveChangesAsync();
        return game;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var game = await _context.Games.FindAsync(id);
        if (game == null)
            return false;

        _context.Games.Remove(game);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<Game>> GetAllAsync()
    {
        return await _context.Games.ToListAsync();
    }

    public async Task<Game?> GetByIdAsync(Guid id)
    {
        return await _context.Games.FindAsync(id);
    }

    public async Task<List<Game>> GetLibraryAsync(Guid userId)
    {
        return await _context.UserGames
            .Where(ug => ug.UserId == userId)
            .Include(ug => ug.Game)
            .Select(ug => ug.Game)
            .ToListAsync();
    }

    public async Task<Game?> UpdateAsync(Guid id, UpdateGameDto dto)
    {
        var game = await _context.Games.FindAsync(id);
        if (game == null)
            return null;

        game.Title = dto.Title.Trim();
        game.Description = dto.Description.Trim();
        game.Price = dto.Price;
        game.Genre = dto.Genre.Trim();
        game.ReleaseDate = dto.ReleaseDate;

        await _context.SaveChangesAsync();
        return game;
    }
}
