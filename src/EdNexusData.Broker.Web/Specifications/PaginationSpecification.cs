using System.Linq.Expressions;
using Ardalis.Specification;

namespace EdNexusData.Broker.Web.Specifications;

public class SearchableWithPaginationSpecification<T> : Specification<T>
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public bool Ascending { get; }
    public List<Expression<Func<T, object>>>? SortExpressions { get; }
    public List<Expression<Func<T, bool>>> SearchExpressions { get; }

    private SearchableWithPaginationSpecification(
        int pageNumber,
        int pageSize,
        bool ascending,
        List<Expression<Func<T, object>>>? sortExpressions,
        List<Expression<Func<T, bool>>> searchExpressions,
        Action<ISpecificationBuilder<T>>? includeAction
    )
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        Ascending = ascending;
        SortExpressions = sortExpressions;
        SearchExpressions = searchExpressions;

        if(pageSize > 0)
        {
            int skip = (pageNumber - 1) * pageSize;
            Query.Skip(skip);
            Query.Take(pageSize);
        }

        if (sortExpressions != null)
        {
            if (Ascending)
            {
                IOrderedSpecificationBuilder<T>? orderQuery = null;
                foreach(var sortExpression in sortExpressions)
                {
                    if (orderQuery is null)
                    {
                        orderQuery = Query.OrderBy(sortExpression!);
                    }
                    else
                    {
                        orderQuery.ThenBy(sortExpression!);
                    }
                }
            }
            else
            {
                IOrderedSpecificationBuilder<T>? orderQuery = null;
                foreach(var sortExpression in sortExpressions)
                {
                    if (orderQuery is null)
                    {
                        orderQuery = Query.OrderByDescending(sortExpression!);
                    }
                    else
                    {
                        orderQuery.ThenByDescending(sortExpression!);
                    }
                }
            }
        }

        foreach (var searchExpression in searchExpressions)
        {
            Query.Where(searchExpression);
        }

        includeAction?.Invoke(Query);
    }

    public class Builder
    {
        private readonly int _pageNumber;
        private readonly int _pageSize;
        private bool _ascending;
        private List<Expression<Func<T, object>>> _sortExpressions;
        private List<Expression<Func<T, bool>>> _searchExpressions;
        private Action<ISpecificationBuilder<T>>? _includeAction;

        public Builder(
            int pageNumber,
            int pageSize)
        {
            _pageNumber = pageNumber;
            _pageSize = pageSize;
            _searchExpressions = new List<Expression<Func<T, bool>>>();
            _sortExpressions = new List<Expression<Func<T, object>>>();
        }

        public Builder WithAscending(bool ascending)
        {
            _ascending = ascending ? ascending : true;
            return this;
        }

        // public Builder WithFilterEducationOrganizationId(string educationOrganizationId)
        // {
        //     _searchExpressions.Add(x => x.EducationOrganizationId == educationOrganizationId);
        //     return this;
        // }

        public Builder WithSortExpression(Expression<Func<T, object>> sortExpression)
        {
            _sortExpressions.Add(sortExpression);
            return this;
        }

        public Builder WithSortExpressions(List<Expression<Func<T, object>>> sortExpression)
        {
            _sortExpressions.AddRange(sortExpression);
            return this;
        }

        public Builder WithSearchExpression(Expression<Func<T, bool>> searchExpression)
        {
            _searchExpressions.Add(searchExpression);
            return this;
        }

        public Builder WithSearchExpressions(List<Expression<Func<T, bool>>> searchExpressions)
        {
            _searchExpressions.AddRange(searchExpressions);
            return this;
        }

        public Builder WithIncludeEntities(Action<ISpecificationBuilder<T>> includeAction)
        {
            _includeAction = includeAction;
            return this;
        }

        public SearchableWithPaginationSpecification<T> Build()
        {
            return new SearchableWithPaginationSpecification<T>(
                _pageNumber,
                _pageSize,
                _ascending,
                _sortExpressions,
                _searchExpressions,
                _includeAction
            );
        }
    }
}
