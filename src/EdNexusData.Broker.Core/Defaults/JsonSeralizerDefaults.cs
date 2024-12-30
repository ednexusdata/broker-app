using System.Text.Json;

namespace EdNexusData.Broker.Core.Defaults;

public static class JsonSerializerDefaults
{
    public static JsonSerializerOptions PropertyNameCaseInsensitive = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };
}