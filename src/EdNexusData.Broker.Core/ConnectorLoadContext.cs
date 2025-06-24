using System.Reflection;
using System.Runtime.Loader;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace EdNexusData.Broker.Core;

public class ConnectorLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public ConnectorLoadContext(string pluginPath) : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        //var path = _resolver.ResolveAssemblyToPath(assemblyName);
        //Console.WriteLine($"Resolving {assemblyName} => {path}");

        // Let the default context handle it
        // if (assemblyName.Name!.StartsWith("Microsoft.AspNetCore") ||
        //       assemblyName.Name.StartsWith("Microsoft.Extensions") ||
        //       assemblyName.Name.StartsWith("System"))
        // {
        //     Console.WriteLine($"Using default context for {assemblyName.Name}");
        //     return AppDomain.CurrentDomain.GetAssemblies()
        //                 .Where(s => s.GetName().Name == assemblyName.Name)
        //                 .FirstOrDefault();
        // }

        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        return assemblyPath != null
            ? LoadFromAssemblyPath(assemblyPath)
            : null;
    }
}
