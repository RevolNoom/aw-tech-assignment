using AwingTest.Server;
using AwingTest.Server.Entities;
using Microsoft.EntityFrameworkCore;

public class AwingTestDbContext : DbContext
{
    public AwingTestDbContext(DbContextOptions<AwingTestDbContext> options)
        : base(options)
    {
    }

    public DbSet<WeatherForecast> WeatherForecasts { get; set; }

    public DbSet<treasure_map> TreasureMaps { get; set; }
}