using Microsoft.EntityFrameworkCore;
using HomeAssistantHelper.Models;

namespace HomeAssistantHelper.Data;

public class AppDbContext : DbContext
{
    public DbSet<ChatSession> Sessions => Set<ChatSession>();
    public DbSet<ChatMessage> Messages => Set<ChatMessage>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // Options are configured via DI; this is only called if not already configured
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Messages)
                  .WithOne(e => e.Session)
                  .HasForeignKey(e => e.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).HasConversion<string>();
        });

        // Settings stored as key-value pairs
        modelBuilder.Entity<SettingEntry>(entity =>
        {
            entity.HasKey(e => e.Key);
        });
    }
}

public class SettingEntry
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
