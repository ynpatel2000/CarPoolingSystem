using System.Linq.Expressions;

namespace Carpooling.Application.Common.Querying;

public static class QueryExtensions
{
    public static IQueryable<T> ApplySearch<T>(
        this IQueryable<T> query,
        string? search,
        Expression<Func<T, bool>> predicate)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        return query.Where(predicate);
    }

    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        SortRequest sort,
        Expression<Func<T, object>> keySelector)
    {
        return sort.Descending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }
}
