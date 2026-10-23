namespace BehaAtel.Application.Common.Models;

public sealed record PaginationQuery
{
    public PaginationQuery(int page = 1, int pageSize = 20, string? search = null, string? sortBy = null, string? sortDir = null)
    {
        Page = Math.Max(1, page);
        PageSize = Math.Clamp(pageSize, 1, 100);
        Search = search;
        SortBy = sortBy;
        SortDir = sortDir;
    }

    public int Page { get; init; }
    public int PageSize { get; init; }
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public string? SortDir { get; init; }
}
