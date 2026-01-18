namespace Carpooling.Application.Common.Pagination;

public static class PaginationExtensions
{
    public static PagedResult<T> ToPagedResult<T>(
        this IQueryable<T> query,
        PagedRequest request)
    {
        var total = query.Count();

        var items = query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<T>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = total
        };
    }
}
