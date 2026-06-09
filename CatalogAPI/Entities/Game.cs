using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CatalogAPI.Entities;

public class Game
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Genre { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    [JsonIgnore]
    public List<UserGame> UserGames { get; set; } = new();
}
