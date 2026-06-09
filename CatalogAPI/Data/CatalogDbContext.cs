using CatalogAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<Promotion> Promotions { get; set; } = null!;
    public DbSet<UserGame> UserGames { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserGame>().HasKey(ug => new { ug.UserId, ug.GameId });
    }
}
