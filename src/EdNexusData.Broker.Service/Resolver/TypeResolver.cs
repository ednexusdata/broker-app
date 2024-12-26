using System.ComponentModel;

namespace EdNexusData.Broker.Service.Resolvers;

public class TypeResolver
{
    private readonly IServiceProvider _serviceProvider;
    
    public TypeResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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