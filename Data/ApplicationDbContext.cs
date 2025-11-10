using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<GameLevel> GameLevels { get; set; }
    public DbSet<Region> Regions { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<GameResult> GameResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
        });

        // Configure relationships
        modelBuilder.Entity<Question>()
            .HasOne<GameLevel>()
            .WithMany()
            .HasForeignKey(q => q.LevelId);

        // Seed data
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "john_doe", Email = "john@example.com" },
            new User { Id = 2, Username = "jane_smith", Email = "jane@example.com" }
        );
    }
}
