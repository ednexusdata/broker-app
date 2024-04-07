using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Web.Models.Searchables;
#nullable disable

namespace EdNexusData.Broker.Web.Models.IncomingRequests;

public class IncomingRequestModel : SearchableModelWithPagination
{
    public string District { get; set; }
    public string School { get; set; }
    public string Student { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    private string _status = string.Empty;

    [Display(Name = "Status")]
    public string Status
    {
        get
        {
            return _status;
        }
        set
        {
            _status = value == "Imported" ? "Approved" : value;
        }
    }

    public Expression<Func<Request, object>> BuildSortExpression()
    {
        Expression<Func<Request, object>> sortExpression = null;
        var sortBy = SortBy.ToLower();
        sortExpression = sortBy switch
        {
            "district" => request => request.EducationOrganization.ParentOrganization.Name,
            "school" => request => request.EducationOrganization.Name,
            "student" => request => request.Student,
            "date" => request => request.InitialRequestSentDate,
            "status" => request => request.RequestStatus,
            _ => request => request.CreatedAt,
        };
        return sortExpression;
    }

    public List<Expression<Func<Request, bool>>> BuildSearchExpressions()
    {
        var searchExpressions = new List<Expression<Func<Request, bool>>>
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

        if (!string.IsNullOrWhiteSpace(District))
        {
            searchExpressions.Add(
                request => request.EducationOrganization.ParentOrganization.Name
                    .ToLower()
                    .Contains(District.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(School))
        {
            searchExpressions.Add(request => request.EducationOrganization.Name
                .ToLower()
                .Contains(School.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(Student))
        {
            searchExpressions.Add(request => request.Student.ToString()
                .ToLower()
                .Contains(Student.ToLower()));
        }

        if (StartDate.HasValue)
        {
            searchExpressions.Add(request => request.CreatedAt >= StartDate.Value);
        }

        if (EndDate.HasValue)
        {
            searchExpressions.Add(request => request.CreatedAt <= EndDate.Value);
        }

        if (Enum.TryParse<RequestStatus>(Status, out var requestStatus))
        {
            searchExpressions.Add(request => request.RequestStatus == requestStatus);
        }

        return searchExpressions;
    }
}
