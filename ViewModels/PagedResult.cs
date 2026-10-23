using Microsoft.EntityFrameworkCore;

namespace AgriEnergyConnect.ViewModels;

public class PagedResult<T>
{
    public const int DefaultPageSize = 12;

    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = DefaultPageSize;

    public int TotalCount { get; init; }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    public static async Task<PagedResult<T>> CreateAsync(IQueryable<T> query, int page, int pageSize = DefaultPageSize)
    {
        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        var safePage = Math.Clamp(page, 1, totalPages);

        var items = await query.Skip((safePage - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<T> { Items = items, Page = safePage, PageSize = pageSize, TotalCount = total };
    }
}
