using EdNexusData.Broker.Core.Specifications;

namespace EdNexusData.Broker.Core.Services;

public class SettingsService
{
    private readonly IRepository<Setting> settingsRepository;
    private readonly IReadRepository<Setting> readSettingsRepository;

    public SettingsService(
        IRepository<Setting> settingsRepository,
        IReadRepository<Setting> readSettingsRepository)
    {
        this.settingsRepository = settingsRepository;
        this.readSettingsRepository = readSettingsRepository;
    }

    public async Task<Setting> GetAsync(string key)
    {
        var setting = await readSettingsRepository.FirstOrDefaultAsync(new SettingByKeySpecification(key));
        if (setting == null)
        {
            setting = new Setting { Key = key, Value = null };
            await settingsRepository.AddAsync(setting);
        }
        return setting;
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var setting = await GetAsync(key);
        return setting.Value;
    }

    public async Task SetValueAsync(string key, string? value)
    {
        var setting = await GetAsync(key);
        setting.Value = value;
        await settingsRepository.UpdateAsync(setting);
    }
}