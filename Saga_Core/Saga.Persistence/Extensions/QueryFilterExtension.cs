using Saga.Domain.Interfaces;
using System.Linq.Expressions;

namespace Saga.Persistence.Extensions;

public static class QueryFilterExtension
{
    public static LambdaExpression GenerateQueryFilter(Type type)
    {
        var parameter = Expression.Parameter(type, "e");

        var falseConstant = Expression.Constant(false);
        var propertyAccess = Expression.PropertyOrField(parameter, nameof(IAuditTrail.DeletedBy));
        // var equalExpression = Expression.Equal(propertyAccess, falseConstant);
        var methodCall = Expression.Call(typeof(string), nameof(string.IsNullOrEmpty), null, propertyAccess);
        var comparation = Expression.NotEqual(Expression.Constant(false), methodCall);

        // var propertyAccess = Expression.PropertyOrField(parameter, nameof(AuditTrailBase.IsDeleted));
        // var comparation = Expression.Equal(Expression.Constant(false), propertyAccess);

        var lambda = Expression.Lambda(comparation, parameter);
        return lambda;
    }
}
