using EdNexusData.Broker.Common.EducationOrganizations;
using Microsoft.Extensions.Logging;

namespace EdNexusData.Broker.Data.Seeds.Populated;

public class InitialSeed
{
    private readonly BrokerDbContext _dbContext;
    private readonly ILogger<SeederService> _logger;

    public InitialSeed(BrokerDbContext dbContext, ILogger<SeederService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await _dbContext.Database.EnsureCreatedAsync();
        
        EducationOrganizations_Oregon();
        EducationOrganizations_Washington();

        _logger.LogInformation("Saving records");

        await _dbContext.SaveChangesAsync();
    }

    private void EducationOrganizations_Oregon()
    {
        // Add Oregon School District
        var edOrg_district = new Core.EducationOrganization {
            Number = "1000",
            Name = "Oregon School District",
            ShortName = "Oregon SD",
            Domain = "or.makoa.dev",
            EducationOrganizationType = EducationOrganizationType.District,
            Address = new Core.Address()
            {
                StreetNumberName = "255 Capitol St NE",
                City = "Salem",
                StateAbbreviation = "OR",
                PostalCode = "97310"
            },
            TimeZone = "America/Los_Angeles",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _dbContext.EducationOrganizations?.Add(edOrg_district);

        // Add Clackamas Elementary School
        var edOrg_school1 = new Core.EducationOrganization {
            Number = "1001",
            Name = "Clackamas Elementary School",
            ShortName = "Clackamas ES",
            ParentOrganization = edOrg_district,
            EducationOrganizationType = EducationOrganizationType.School,
            Address = new Core.Address()
            {
                StreetNumberName = "1710 Red Soils Ct, Suite 100",
                City = "Oregon City",
                StateAbbreviation = "OR",
                PostalCode = "97045"
            },
            Contacts = new List<Core.EducationOrganizationContact>()
            {
                new Core.EducationOrganizationContact()
                {
                    Name = "Catherine McMullen",
                    Email = "catherinemcmullen@clackamas.us",
                    JobTitle = "County Clerk",
                    Phone = "503-722-6086"
                }
            }
        };
        _dbContext.EducationOrganizations?.Add(edOrg_school1);

        // Add Multnomah Middle School
        var edOrg_school2 = new Core.EducationOrganization {
            Number = "1002",
            Name = "Multnomah Middle School",
            ShortName = "Multnomah MS",
            ParentOrganization = edOrg_district,
            EducationOrganizationType = EducationOrganizationType.School,
            Address = new Core.Address()
            {
                StreetNumberName = "1620 SE 190th Ave",
                City = "Portland",
                StateAbbreviation = "OR",
                PostalCode = "97233"
            },
            Contacts = new List<Core.EducationOrganizationContact>()
            {
                new Core.EducationOrganizationContact()
                {
                    Name = "Deidre Thieman",
                    Email = "deidre.thieman@multco.us",
                    JobTitle = "County Archivist and Records Program Supervisor",
                    Phone = "503-988-3741"
                }
            }
        };
        _dbContext.EducationOrganizations?.Add(edOrg_school2);

        // Add Yamhill High School
        var edOrg_school3 = new Core.EducationOrganization {
            Number = "1003",
            Name = "Yamhill High School",
            ShortName = "Yamhill HS",
            ParentOrganization = edOrg_district,
            EducationOrganizationType = EducationOrganizationType.School,
            Address = new Core.Address()
            {
                StreetNumberName = "535 NE 5th Str",
                City = "McMinnville",
                StateAbbreviation = "OR",
                PostalCode = "97128"
            },
            Contacts = new List<Core.EducationOrganizationContact>()
            {
                new Core.EducationOrganizationContact()
                {
                    Name = "Keri Hinton",
                    Email = "clerk@co.yamhill.or.us",
                    JobTitle = "Yamhill County Clerk",
                    Phone = "503-474-7518"
                }
            }
        };
        _dbContext.EducationOrganizations?.Add(edOrg_school3);
    }

    private void EducationOrganizations_Washington()
    {
        // Add Washington School District
        var edOrg_district = new Core.EducationOrganization {
            Number = "2000",
            Name = "Washington School District",
            ShortName = "Washington SD",
            Domain = "wa.makoa.dev",
            EducationOrganizationType = EducationOrganizationType.District,
            Address = new Core.Address()
            {
                StreetNumberName = "600 Washington St SE",
                City = "Olympia",
                StateAbbreviation = "WA",
                PostalCode = "98504"
            },
            TimeZone = "America/Los_Angeles",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _dbContext.EducationOrganizations?.Add(edOrg_district);

        // Add Cowlitz Elementary School
        var edOrg_school1 = new Core.EducationOrganization {
            Number = "2001",
            Name = "Cowlitz Elementary School",
            ShortName = "Cowlitz ES",
            ParentOrganization = edOrg_district,
            EducationOrganizationType = EducationOrganizationType.School,
            Address = new Core.Address()
            {
                StreetNumberName = "207 4th Ave. N.",
                City = "Kelso",
                StateAbbreviation = "WA",
                PostalCode = "98626"
            },
            Contacts = new List<Core.EducationOrganizationContact>()
            {
                new Core.EducationOrganizationContact()
                {
                    Name = "Bailey Silva",
                    Email = "publicrecords@cowlitzwa.gov",
                    JobTitle = "Public Records Officer",
                    Phone = "360-577-3020 ext. 6990"
                }
            }
        };
        _dbContext.EducationOrganizations?.Add(edOrg_school1);

        // Add Pacific Middle School
        var edOrg_school2 = new Core.EducationOrganization {
            Number = "2002",
            Name = "Pacific Middle School",
            ShortName = "Pacific MS",
            ParentOrganization = edOrg_district,
            EducationOrganizationType = EducationOrganizationType.School,
            Address = new Core.Address()
            {
                StreetNumberName = "300 Memorial Drive",
                City = "South Bend",
                StateAbbreviation = "WA",
                PostalCode = "98586"
            },
            Contacts = new List<Core.EducationOrganizationContact>()
            {
                new Core.EducationOrganizationContact()
                {
                    Name = "Alex Gerow",
                    Email = "auditor@co.pacific.wa.us",
                    JobTitle = "County Auditor",
                    Phone = "360-577-3020 ext. 6990"
                }
            }
        };
        _dbContext.EducationOrganizations?.Add(edOrg_school2);

        // Add Skamania High School
        var edOrg_school3 = new Core.EducationOrganization {
            Number = "2003",
            Name = "Skamania High School",
            ShortName = "Skamania HS",
            ParentOrganization = edOrg_district,
            EducationOrganizationType = EducationOrganizationType.School,
            Address = new Core.Address()
            {
                StreetNumberName = "240 NW Vancouver Ave",
                City = "Stevenson",
                StateAbbreviation = "WA",
                PostalCode = "98648"
            },
            Contacts = new List<Core.EducationOrganizationContact>()
            {
                new Core.EducationOrganizationContact()
                {
                    Name = "Grace D. Cross",
                    Email = "cross@co.skamania.wa.us",
                    JobTitle = "County Clerk & Clerk of the Superior Court",
                    Phone = "509-427-3771"
                }
            }
        };
        _dbContext.EducationOrganizations?.Add(edOrg_school3);
    }
}