using HomeAssistantHelper.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeAssistantHelper.Tests.Helpers;

public static class DbHelper
{
    public static IDbContextFactory<AppDbContext> CreateInMemoryFactory()
    {
        var dbName = $"TestDb_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={dbName};Mode=Memory;Cache=Shared")
            .Options;

        // Open a persistent connection to keep the in-memory DB alive
        var keepAlive = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbName};Mode=Memory;Cache=Shared");
        keepAlive.Open();

        using (var ctx = new AppDbContext(options))
        {
            ctx.Database.EnsureCreated();
        }

        return new TestDbContextFactory(options, keepAlive);
    }
}

internal class TestDbContextFactory(
    DbContextOptions<AppDbContext> options,
    Microsoft.Data.Sqlite.SqliteConnection keepAlive) : IDbContextFactory<AppDbContext>, IDisposable
{
    public AppDbContext CreateDbContext() => new(options);

    public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(CreateDbContext());

    public void Dispose() => keepAlive.Dispose();
}
