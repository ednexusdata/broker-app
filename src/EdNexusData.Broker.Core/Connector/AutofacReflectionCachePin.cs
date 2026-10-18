using System.Runtime.CompilerServices;
using Autofac.Core;

namespace EdNexusData.Broker.Core;

/// <summary>
/// Keeps Autofac's shared reflection cache set alive for the life of the process.
/// </summary>
/// <remarks>
/// Autofac holds <see cref="ReflectionCacheSet.Shared"/> only through a weak reference and creates a new one if it
/// is collected. Caches registered with the old set, such as Autofac.Extensions.DependencyInjection's
/// FromKeyedServicesUsageCache (registered once, from a static constructor), are then no longer cleared when a
/// connector unloads, and their entries keep the old connector version in memory. This runs when this assembly
/// loads, before any container is built, so those caches register with the set kept here.
/// </remarks>
internal static class AutofacReflectionCachePin
{
    private static ReflectionCacheSet? _shared;

    internal static ReflectionCacheSet? Shared => _shared;

#pragma warning disable CA2255 // Must run before Autofac caches register with the shared set.
    [ModuleInitializer]
#pragma warning restore CA2255
    internal static void Pin() => _shared = ReflectionCacheSet.Shared;
}
