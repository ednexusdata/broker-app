using EdNexusData.Broker.Domain;
using System.Linq.Expressions;
using EdNexusData.Broker.Web.Models.Searchables;
using System.Net.NetworkInformation;

namespace EdNexusData.Broker.Web.Models.Users;

public class UserRequestModel : SearchableModelWithPagination
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Type { get; set; } = default!;

    public Expression<Func<User, object>> BuildSortExpression()
    {
        Expression<Func<User, object>>? sortExpression = null;
        var sortBy = SortBy.ToLower();
        sortExpression = sortBy switch
        {
            "firstname" => request => request.FirstName,
            "lastname" => request => request.LastName,
            "issuperadmin" => request => request.IsSuperAdmin,
            "permissiontype" => request => request.AllEducationOrganizations,
            _ => request => request.LastName,
        };
        return sortExpression;
    }

    public List<Expression<Func<User, bool>>> BuildSearchExpressions()
    {
        var searchExpressions = new List<Expression<Func<User, bool>>>();

        if (!string.IsNullOrWhiteSpace(SearchBy))
        {
            searchExpressions.Add(
                request => request.FirstName
                    .ToLower()
                    .Contains(SearchBy.ToLower())
                || request.LastName
                    .ToLower()
                    .Contains(SearchBy.ToLower())
            );
        }

        if (!string.IsNullOrWhiteSpace(FirstName))
        {
            searchExpressions.Add(
                request => request.FirstName
                    .ToLower()
                    .Contains(FirstName.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(LastName))
        {
            searchExpressions.Add(request => request.LastName
                .ToLower()
                .Contains(LastName.ToLower()));
        }

        if (Enum.TryParse<PermissionType>(Type, out var requestStatus))
        {
            searchExpressions.Add(request => request.AllEducationOrganizations == requestStatus);
        }

        return searchExpressions;
    }
}

