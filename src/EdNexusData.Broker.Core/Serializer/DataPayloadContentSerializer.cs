using System.Text.Json;
using System.Text.Json.Nodes;
using EdNexusData.Broker.Common.PayloadContents;
using EdNexusData.Broker.Common.Payloads;
using Newtonsoft.Json;

namespace EdNexusData.Broker.Core.Serializers;

public static class DataPayloadContentSerializer
{
    public static DataPayloadContent Deseralize(string jsonString)
    {
        // Deserialize Schema
        var json = JsonDocument.Parse(jsonString);
        var payloadContentSchemaJson = json.RootElement.GetProperty("Schema");
        var payloadContentSchema = System.Text.Json.JsonSerializer.Deserialize<PayloadContentSchema>(payloadContentSchemaJson.ToString()!);
        _ = payloadContentSchema ?? throw new NullReferenceException("Schema missing");
        
        // Create type object
        var payloadContentSchemaType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetExportedTypes())
                    .Where(p => p.FullName == payloadContentSchema.ObjectType).FirstOrDefault();
        _ = payloadContentSchemaType ?? throw new NullReferenceException($"Unable to create type from schema type {payloadContentSchema.ObjectType}");
        
        // Get the AdditionalContents node
        JsonElement root = json.RootElement;
        JsonElement additionalNode = root.GetProperty("AdditionalContents");

        // Reconstruct the JSON without the "AdditionalContents" node
        JsonNode rootNode = JsonNode.Parse(json.ToJsonString()!)!;
        JsonNode additionalRootNode = rootNode["AdditionalContents"]!;
        rootNode["AdditionalContents"] = null;

        var jsondeserialize = JsonConvert.DeserializeObject(rootNode.ToJsonString()!, payloadContentSchemaType);
        DataPayloadContent payloadContentObject = (jsondeserialize as DataPayloadContent)!;
        
        if (additionalRootNode is not null)
        {
            if (additionalRootNode.GetValueKind() == JsonValueKind.Array)
            {
                List<object> additionalContents = System.Text.Json.JsonSerializer.Deserialize<List<object>>(additionalRootNode.ToJsonString())!;
                if (additionalContents.Count > 0)
                {
                    foreach(var additionalContent in additionalContents)
                    {
                        var result = Deseralize(System.Text.Json.JsonSerializer.Serialize(additionalContent));
                        if (result is not null && payloadContentObject.AdditionalContents is null)
                        {
                            payloadContentObject.AdditionalContents = new List<DataPayloadContent>();
                        } 
                        if (result is not null)
                        {
                            payloadContentObject.AdditionalContents?.Add(result);
                        }
                    }
                }
            }
            else
            {
                var result = Deseralize(additionalRootNode.ToJsonString());
                if (result is not null && payloadContentObject.AdditionalContents is null)
                {
                    payloadContentObject.AdditionalContents = new List<DataPayloadContent>();
                } 
                if (result is not null)
                {
                    payloadContentObject.AdditionalContents?.Add(result);
                }
            }
        }

        return payloadContentObject;
    }
}