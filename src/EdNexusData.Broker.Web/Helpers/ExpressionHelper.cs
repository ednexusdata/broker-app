using System.Linq.Expressions;

namespace EdNexusData.Broker.Web.Helpers;

public class ExpressionHelper
{
    public Expression<Func<T, object>> GetSortExpression<T>(string propertyPath) where T : class
    {
        if (propertyPath.Contains('.'))
        {
            return GetNestedPropertyExpression<T>(propertyPath);
        }
        else
        {
            return GetSinglePropertyExpression<T>(propertyPath);
        }
    }

    private static Expression<Func<T, object>> GetSinglePropertyExpression<T>(string propertyName) where T : class
    {
        var parameter = Expression.Parameter(typeof(T));
        return Expression.Lambda<Func<T, object>>(
            Expression.Convert(Expression.Property(parameter, propertyName), typeof(object)),
            parameter);
    }

    private static Expression<Func<T, object>> GetNestedPropertyExpression<T>(string propertyPath) where T : class
    {
        var parameter = Expression.Parameter(typeof(T));
        Expression propertyAccess = parameter;

        foreach (var propertyName in propertyPath.Split('.'))
        {
            propertyAccess = Expression.Property(propertyAccess, propertyName);
        }

        return Expression.Lambda<Func<T, object>>(
            Expression.Convert(propertyAccess, typeof(object)),
            parameter);
    }
}
