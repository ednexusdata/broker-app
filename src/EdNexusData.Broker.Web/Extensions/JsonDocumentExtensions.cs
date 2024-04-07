using System.Text.Json;

namespace EdNexusData.Broker.Web;

public static class JsonDocumentExtensions
{
    public static JsonDocument ToJsonDocument(this object model)
    {
        string jsonString = JsonSerializer.Serialize(model);

        return JsonDocument.Parse(jsonString);
    }

    public static T? DeserializeFromJsonDocument<T>(this JsonDocument jsonDocument) where T : class
    {
        if (jsonDocument is null)
        {
            throw new ArgumentNullException(nameof(jsonDocument));
        }

        var jsonString = jsonDocument.RootElement.GetRawText();

        try
        {
            return JsonSerializer.Deserialize<T>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true 
            });
        }
        catch 
        {
            return null;
        }
    }
}
