using HomeAssistantHelper.Data;
using HomeAssistantHelper.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeAssistantHelper.Services;

public class SettingsService(IDbContextFactory<AppDbContext> dbFactory)
{
    private const string EncryptionScope = "HomeAssistantHelper";

    public async Task<AppSettings> LoadAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var settings = new AppSettings();
        var entries = await db.Set<SettingEntry>().ToListAsync();
        var dict = entries.ToDictionary(e => e.Key, e => e.Value);

        if (dict.TryGetValue("McpServerUrl", out var url))
            settings.McpServerUrl = url;
        if (dict.TryGetValue("CopilotModel", out var model))
            settings.CopilotModel = model;

        return settings;
    }

    public async Task SaveAsync(AppSettings settings)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        await Upsert(db, "McpServerUrl", settings.McpServerUrl);
        await Upsert(db, "CopilotModel", settings.CopilotModel);

        await db.SaveChangesAsync();
    }

    private static async Task Upsert(AppDbContext db, string key, string value)
    {
        var entry = await db.Set<SettingEntry>().FindAsync(key);
        if (entry is null)
        {
            db.Set<SettingEntry>().Add(new SettingEntry { Key = key, Value = value });
        }
        else
        {
            entry.Value = value;
        }
    }
}
