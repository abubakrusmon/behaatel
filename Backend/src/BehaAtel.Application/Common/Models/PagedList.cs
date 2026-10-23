using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Common.Models;

public sealed class PagedList<T>
{
    private PagedList(IReadOnlyCollection<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public IReadOnlyCollection<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var (normalizedPage, normalizedPageSize) = Normalize(page, pageSize);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<T>(items, normalizedPage, normalizedPageSize, totalCount);
    }

    public static PagedList<T> Create(IEnumerable<T> source, int page, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        var (normalizedPage, normalizedPageSize) = Normalize(page, pageSize);
        var items = source.ToArray();
        var pageItems = items
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToArray();

        return new PagedList<T>(pageItems, normalizedPage, normalizedPageSize, items.Length);
    }

    public static PagedList<T> CreatePage(IEnumerable<T> items, int page, int pageSize, int totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (totalCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalCount), "Общее количество не может быть отрицательным.");
        }

        var (normalizedPage, normalizedPageSize) = Normalize(page, pageSize);
        return new PagedList<T>(items.ToArray(), normalizedPage, normalizedPageSize, totalCount);
    }

    private static (int Page, int PageSize) Normalize(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Номер страницы должен быть не меньше 1.");
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Размер страницы должен быть не меньше 1.");
        }

        return (page, Math.Min(pageSize, 100));
    }
}
