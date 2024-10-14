using System.ComponentModel;
using Ardalis.GuardClauses;

namespace EdNexusData.Broker.Service.Extensions;

public class Description
{
    public static string? Get(string? typeFullName)
    {
        Guard.Against.Null(typeFullName, "typeFullName", "Missing type to get.");

        var resolvedType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetExportedTypes())
            .Where(p => p.FullName == typeFullName)
            .FirstOrDefault();
        
        Guard.Against.Null(resolvedType, "resolvedType", "Unable to get type");
        
        var descriptionObject = resolvedType.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DescriptionAttribute)).FirstOrDefault();

        Guard.Against.Null(descriptionObject, "descriptionObject", "Missing description attribute.");

        if (descriptionObject is not null)
        {
            return ((DescriptionAttribute)descriptionObject).Description;
        }

        return null;
    }

    public static string? Find(string? typeFullName)
    {
        if (typeFullName is null) return null;

        var resolvedType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetExportedTypes())
            .Where(p => p.FullName == typeFullName)
            .FirstOrDefault();
        
        if (resolvedType is null) return null;
        
        var descriptionObject = resolvedType.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DescriptionAttribute)).FirstOrDefault();

        if (descriptionObject is null) return null;

        if (descriptionObject is not null)
        {
            return ((DescriptionAttribute)descriptionObject).Description;
        }

        return null;
    }

    public static string? ResolveFind(string? typeFullname)
    {
        var find = Find(typeFullname);
        return find ?? typeFullname;
    }
}