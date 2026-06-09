using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CatalogAPI.DTOs;
using CatalogAPI.Services;
using FiapCloudGames.Events;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("api/games")]
[Authorize]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly IPublishEndpoint _publishEndpoint;

    public GameController(IGameService gameService, IPublishEndpoint publishEndpoint)
    {
        _gameService = gameService;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ListAllGames()
    {
        var games = await _gameService.GetAllAsync();
        return Ok(games.Select(g => new GameResponseDto
        {
            Id = g.Id,
            Title = g.Title,
            Description = g.Description,
            Price = g.Price,
            Genre = g.Genre,
            ReleaseDate = g.ReleaseDate
        }));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGameById(Guid id)
    {
        var game = await _gameService.GetByIdAsync(id);
        if (game == null)
            return NotFound("Jogo não encontrado");

        return Ok(new GameResponseDto
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            Price = game.Price,
            Genre = game.Genre,
            ReleaseDate = game.ReleaseDate
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameDto dto)
    {
        var game = await _gameService.CreateAsync(dto);
        var response = new GameResponseDto
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            Price = game.Price,
            Genre = game.Genre,
            ReleaseDate = game.ReleaseDate
        };
        return CreatedAtAction(nameof(GetGameById), new { id = game.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateGameById(Guid id, [FromBody] UpdateGameDto dto)
    {
        var game = await _gameService.UpdateAsync(id, dto);
        if (game == null)
            return NotFound("Jogo não encontrado");

        return Ok(new GameResponseDto
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            Price = game.Price,
            Genre = game.Genre,
            ReleaseDate = game.ReleaseDate
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteGameById(Guid id)
    {
        if (!await _gameService.DeleteAsync(id))
            return NotFound("Jogo não encontrado");

        return NoContent();
    }

    [HttpPost("{id}/buy")]
    public async Task<IActionResult> BuyGame(Guid id)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized("Usuário inválido");

        var game = await _gameService.GetByIdAsync(id);
        if (game == null)
            return NotFound("Jogo não encontrado");

        // Assuming basic check to prevent buying twice, though typically handled via events or read model
        var library = await _gameService.GetLibraryAsync(userId);
        if (library.Any(g => g.Id == id))
            return BadRequest("Usuário já possui este jogo.");

        await _publishEndpoint.Publish(new OrderPlacedEvent(userId, game.Id, game.Price));

        return Accepted(new { message = $"Pedido de compra do jogo '{game.Title}' recebido com sucesso!" });
    }

    [HttpGet("library")]
    public async Task<IActionResult> GetMyLibrary()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized("Usuário inválido");

        var games = await _gameService.GetLibraryAsync(userId);
        return Ok(games.Select(g => new GameResponseDto
        {
            Id = g.Id,
            Title = g.Title,
            Description = g.Description,
            Price = g.Price,
            Genre = g.Genre,
            ReleaseDate = g.ReleaseDate
        }));
    }
}
