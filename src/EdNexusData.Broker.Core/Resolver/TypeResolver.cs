using System.ComponentModel;
using System.Reflection;
using System.Runtime.Loader;

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
            .ToList().FirstOrDefault();

        _ = type
            ?? throw new InvalidOperationException($"Type '{typeName}' not found in loaded connectors.");
        
        var assembly = type.Assembly;
        var context = AssemblyLoadContext.GetLoadContext(assembly);
        Console.WriteLine($"**************Assembly: {assembly.FullName}");
        Console.WriteLine($"**************Load Context: {context?.Name ?? "Default"}");

        return type;
    }

    public Type ResolveConnectorTypeInContext(string typeName, Assembly assembly)
    {
        // Find the context
        var contextName = AssemblyLoadContext.GetLoadContext(assembly)?.Name;
        _ = contextName
            ?? throw new InvalidOperationException($"Assembly '{assembly.FullName}' is not loaded in any connector context.");

        // Get the context
        var context = connectorLoader.ConnectorLoadContexts
            .Where(c => c.Key == contextName).FirstOrDefault().Value;

        var type = context.Assemblies
                .Where(inner => inner.GetType(typeName) != null)
                .Select(inner => inner.GetType(typeName))
                .ToList().FirstOrDefault();

        _ = type
            ?? throw new InvalidOperationException($"Type '{typeName}' not found in loaded connectors.");
        
        var assemblyLog = type.Assembly;
        var contextLog = AssemblyLoadContext.GetLoadContext(assemblyLog);
        Console.WriteLine($"**************Assembly: {assemblyLog.FullName}");
        Console.WriteLine($"**************Load Context: {contextLog?.Name ?? "Default"}");

        return type;
    }

    public List<Type>? ResolveConnectorInterface(string typeName)
    {
        var types = connectorLoader.ConnectorLoadContexts
            .SelectMany(outer => outer.Value.Assemblies
                .SelectMany(s => s.GetExportedTypes())
                .Where(inner => inner.GetInterface(typeName) is not null))
            .ToList();

        if (types is not null && types.Count > 0)
        {
            foreach (var type in types)
            {
                var assembly = type.Assembly;
                var context = AssemblyLoadContext.GetLoadContext(assembly);
                Console.WriteLine($"**************Assembly: {assembly.FullName}");
                Console.WriteLine($"**************Load Context: {context?.Name ?? "Default"}");
            }
        }

        return types
            ?? throw new InvalidOperationException($"Interface '{typeName}' not found in loaded connectors.");
    }

    public List<Type>? ResolveConnectorInterface(Assembly assembly, string typeName)
    {
        var types = connectorLoader.ConnectorLoadContexts
            .Where(outer => outer.Value.Assemblies.Any(a => a == assembly))
            .FirstOrDefault().Value.Assemblies
            .SelectMany(s => s.GetExportedTypes())
            .Where(inner => inner.GetInterface(typeName) is not null)
            .ToList();

        if (types is not null && types.Count > 0)
        {
            foreach (var type in types)
            {
                var assemblyText = type.Assembly;
                var context = AssemblyLoadContext.GetLoadContext(assemblyText);
                Console.WriteLine($"**************Assembly: {assemblyText.FullName}");
                Console.WriteLine($"**************Load Context: {context?.Name ?? "Default"}");
            }
        }

        return types
            ?? throw new InvalidOperationException($"Interface '{typeName}' not found in loaded connectors.");
    }

    public static List<Type>? ResolveConnectorInterface(ConnectorLoader connectorLoader, string typeName)
    {
        var types = connectorLoader.ConnectorLoadContexts
            .SelectMany(outer => outer.Value.Assemblies
                .SelectMany(s => s.GetExportedTypes())
                .Where(inner => inner.GetInterface(typeName) is not null))
            .ToList();

        if (types is not null && types.Count > 0)
        {
            foreach (var type in types)
            {
                var assembly = type.Assembly;
                var context = AssemblyLoadContext.GetLoadContext(assembly);
                Console.WriteLine($"**************Assembly: {assembly.FullName}");
                Console.WriteLine($"**************Load Context: {context?.Name ?? "Default"}");
            }
        }

        return types
            ?? throw new InvalidOperationException($"Interface '{typeName}' not found in loaded connectors.");
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