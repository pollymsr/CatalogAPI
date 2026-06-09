using System;
using System.Text.Json.Serialization;

namespace CatalogAPI.Entities;

public class UserGame
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    [JsonIgnore]
    public Game Game { get; set; } = null!;
    public DateTime PurchaseDate { get; set; }
}
