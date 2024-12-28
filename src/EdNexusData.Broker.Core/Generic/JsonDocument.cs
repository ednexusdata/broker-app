using System.Text;
using System.Text.Json;

namespace EdNexusData.Broker.Core;

public static class JsonHelpers
{
    public static string? ToJsonString(this JsonDocument? jdoc)
    {
        if (jdoc is not null)
        {
            using (var stream = new MemoryStream())
            {
                Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
                jdoc.WriteTo(writer);
                writer.Flush();
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
        else
        {
            return null;
        }
    }

    public static JsonDocument? FromJsonString(this string? jsonString)
    {
        if (jsonString is not null)
        {
            return JsonSerializer.Deserialize<JsonDocument>(jsonString);
        }
        else
        {
            return null;
        }
    }
}