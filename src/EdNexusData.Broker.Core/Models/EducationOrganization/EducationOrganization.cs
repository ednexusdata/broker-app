// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
using System.ComponentModel.DataAnnotations.Schema;
using EdNexusData.Broker.Common.EducationOrganizations;
using Microsoft.Extensions.Caching.Memory;

namespace EdNexusData.Broker.Core;

public class EducationOrganization : BaseEntity, IAggregateRoot
{
    public EducationOrganization? ParentOrganization { get; set; }
    public Guid? ParentOrganizationId { get; set; }
    public string Name { get; set; } = default!;
    public string ShortName { get; set; } = default!;
    
    [NotMapped]
    public string FullName { 
        get { 
            Func<EducationOrganization, string, string> nameRecursion = null!;
            var collectedName = "";

            nameRecursion = (org, collectedName) => 
            {
                if (org.ParentOrganization != null)
                {
                    collectedName = " / " + org.Name + collectedName;
                    return nameRecursion(org.ParentOrganization, collectedName);
                }
                else
                {
                    return org.Name + collectedName;
                }
            };
            return nameRecursion(this, collectedName);
        } 
    }

    public string? Number { get; set; }
    public string? StateCode { get; set; }
    public string? NcesCode { get; set; }
    public string? CeebCode { get; set; }
    public EducationOrganizationType EducationOrganizationType { get; set; } = EducationOrganizationType.District;
    public Address? Address { get; set; }
    public string? Domain { get; set; }
    public string? TimeZone { get; set; }
    public List<EducationOrganizationContact>? Contacts { get; set; }

    public ICollection<EducationOrganization>? EducationOrganizations { get; set; }

    public virtual ICollection<EducationOrganization> ChildEducationOrganizations { get; set; } = new List<EducationOrganization>();

    public Common.EducationOrganizations.EducationOrganization ToCommon()
    {
        var educationOrganization = new Common.EducationOrganizations.EducationOrganization()
        {
            Id = Id,
            ParentOrganizationId = ParentOrganizationId,
            Name = Name,
            ShortName = ShortName,
            Number = Number,
            CeebCode = CeebCode,
            NcesCode = NcesCode,
            StateCode = StateCode,
            EducationOrganizationType = EducationOrganizationType
        };

        return educationOrganization;
    }

    public static List<EducationOrganization>? EducationOrganizationsWithHierarchy(List<EducationOrganization> orgs, IMemoryCache? memoryCache = null)
    {
        if (memoryCache != null 
            && memoryCache.TryGetValue<List<EducationOrganization>>(CachedRepository<EducationOrganization>.LastCachedValue, out var cachedOrgs))
        {
            return cachedOrgs;
        }

        var hierarchy = BuildHierarchy(orgs, memoryCache);

        if (memoryCache != null)
        {
            memoryCache.Set(CachedRepository<EducationOrganization>.LastCachedValue, hierarchy, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
        }

        return hierarchy;
    }

    public static List<EducationOrganization> BuildHierarchy(List<EducationOrganization> orgs, IMemoryCache? memoryCache = null)
    {
        var lookup = orgs.ToDictionary(o => o.Id, o => new EducationOrganization
        {
            Id = o.Id,
            Name = o.Name,
            EducationOrganizationType = o.EducationOrganizationType
        });

        var roots = new List<EducationOrganization>();

        foreach (var org in orgs)
        {
            if (org.ParentOrganizationId is Guid parentId && lookup.ContainsKey(parentId))
            {
                lookup[parentId].ChildEducationOrganizations?.Add(lookup[org.Id]);
            }
            else
            {
                roots.Add(lookup[org.Id]);
            }
        }

        return roots;
    }
}
