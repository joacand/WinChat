namespace WinChat.Infrastructure.Repository;

public class ColorSettingsRepository(AppDbContext context)
{
    public async Task SaveColorSettingsAsync(ColorSettingsData colorSettings)
    {
        var settings = new List<ApplicationData>
        {
            new() { SettingKey = "ForegroundColorHex", SettingValue = colorSettings.ForegroundColorHex },
            new() { SettingKey = "BackgroundColorHex", SettingValue = colorSettings.BackgroundColorHex },
            new() { SettingKey = "AssistantChatColorHex", SettingValue = colorSettings.AssistantChatColorHex },
            new() { SettingKey = "UserChatColorHex", SettingValue = colorSettings.UserChatColorHex }
        };

        foreach (var setting in settings)
        {
            var existingSetting = await context.ApplicationData.FindAsync(setting.SettingKey);
            if (existingSetting != null)
            {
                existingSetting.SettingValue = setting.SettingValue;
                context.ApplicationData.Update(existingSetting);
            }
            else
            {
                await context.ApplicationData.AddAsync(setting);
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task<ColorSettingsData?> LoadColorSettingsAsync()
    {
        var colorSettings = new ColorSettingsData
        {
            ForegroundColorHex = await GetSettingValueAsync("ForegroundColorHex"),
            BackgroundColorHex = await GetSettingValueAsync("BackgroundColorHex"),
            AssistantChatColorHex = await GetSettingValueAsync("AssistantChatColorHex"),
            UserChatColorHex = await GetSettingValueAsync("UserChatColorHex")
        };

        return colorSettings;
    }

    private async Task<string?> GetSettingValueAsync(string key)
    {
        var setting = await context.ApplicationData.FindAsync(key);
        return setting?.SettingValue;
    }
}
