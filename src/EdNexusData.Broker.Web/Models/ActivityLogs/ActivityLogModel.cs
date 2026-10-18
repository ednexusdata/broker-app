using System.Linq.Expressions;
using EdNexusData.Broker.Web.Models.Searchables;
#nullable disable

namespace EdNexusData.Broker.Web.Models.ActivityLogs;

public class ActivityLogModel : SearchableModelWithPagination
{
    public Expression<Func<ActivityLog, object>> BuildSortExpression()
    {
        Expression<Func<ActivityLog, object>> sortExpression;
        var sortBy = SortBy?.ToLower();
        sortExpression = sortBy switch
        {
            "user" => log => log.User.LastName,
            "activitytype" => log => log.ActivityType,
            _ => log => log.CreatedAt,
        };
        return sortExpression;
    }

    public List<Expression<Func<ActivityLog, bool>>> BuildSearchExpressions()
    {
        var searchExpressions = new List<Expression<Func<ActivityLog, bool>>>();

        if (!string.IsNullOrWhiteSpace(SearchBy))
        {
            var searchByLower = SearchBy.ToLower();
            searchExpressions.Add(
                log => (log.Description != null && log.Description.ToLower().Contains(searchByLower))
                    || log.Action.ToLower().Contains(searchByLower)
                    || (log.User != null && (log.User.FirstName.ToLower().Contains(searchByLower) || log.User.LastName.ToLower().Contains(searchByLower)))
            );
        }

        return searchExpressions;
    }
}
