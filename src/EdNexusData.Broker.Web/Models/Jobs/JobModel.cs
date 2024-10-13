using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Worker;
using EdNexusData.Broker.Web.Models.Searchables;
#nullable disable

namespace EdNexusData.Broker.Web.Models.Jobs;

public class JobModel : SearchableModelWithPagination
{
    public string Id { get; set; }
    public DateTime? QueuedDateTime { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? FinishDateTime { get; set; }
    public JobStatus? JobStatus { get; set; }
    public string JobType { get; set; }
    private string _status = string.Empty;

    public Expression<Func<Job, object>> BuildSortExpression()
    {
        Expression<Func<Job, object>> sortExpression = null;
        var sortBy = SortBy.ToLower();
        sortExpression = sortBy switch
        {
            // "district" => request => request.EducationOrganization.ParentOrganization.Name,
            // "school" => request => request.EducationOrganization.Name,
            // "student" => request => request.Student,
            // "date" => request => request.InitialRequestSentDate,
            // "status" => request => request.RequestStatus,
            _ => job => job.QueueDateTime,
        };
        return sortExpression;
    }

    public List<Expression<Func<Job, bool>>> BuildSearchExpressions()
    {
        var searchExpressions = new List<Expression<Func<Job, bool>>>
        {
            //request => request.RequestProcessUserId.HasValue
        };

        if (!string.IsNullOrWhiteSpace(SearchBy))
        {
            var searchByLower = SearchBy.ToLower();
            //todo: include student here..remove from controller.
            //searchExpressions.Add(
            //    request => request.EducationOrganization.ParentOrganization.Name
            //        .ToLower()
            //        .Contains(searchByLower)
            //    || request.EducationOrganization.Name
            //        .ToLower()
            //        .Contains(searchByLower)
            //    || request.Student.FirstName
            //        .ToLower()
            //        .Contains(searchByLower)
            //    || request.Student.LastName
            //        .ToLower()
            //        .Contains(searchByLower)
            //);
        }

        return searchExpressions;
    }
}
