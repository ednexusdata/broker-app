using System.ComponentModel;

namespace EdNexusData.Broker.Core.Resolvers;

public class TypeResolver
{
    private readonly ConnectorLoader connectorLoader;

    public TypeResolver(
        ConnectorLoader connectorLoader)
    {
        this.connectorLoader = connectorLoader;
    }
    
    public Type ResolveConnectorType(string typeName)
    {
        var type = connectorLoader.ConnectorLoadContexts
            .SelectMany(outer => outer.Value.Assemblies
                .Where(inner => inner.GetType(typeName) != null)
                .Select(inner => inner.GetType(typeName)))
            .ToList();

        return type.FirstOrDefault()
            ?? throw new InvalidOperationException($"Type '{typeName}' not found in loaded connectors.");
    }

    public static DisplayNameAttribute? ResolveDisplayName(string typeName)
    {
        var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetExportedTypes())
                .Where(p => p.FullName == typeName)
                .FirstOrDefault();

        if (type is null)
        {
            return null;
        }

        return type
            .GetCustomAttributes(false)
            .Where(x => x.GetType() == typeof(DisplayNameAttribute))
            .FirstOrDefault()! as DisplayNameAttribute;
    }
}