using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatalogAPI.DTOs;
using CatalogAPI.Entities;

namespace CatalogAPI.Services;

public interface IGameService
{
    Task<Game> CreateAsync(CreateGameDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<List<Game>> GetAllAsync();
    Task<Game?> GetByIdAsync(Guid id);
    Task<List<Game>> GetLibraryAsync(Guid userId);
    Task<Game?> UpdateAsync(Guid id, UpdateGameDto dto);
}
