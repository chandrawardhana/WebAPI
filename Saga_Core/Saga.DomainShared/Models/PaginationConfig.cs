using Saga.DomainShared.Enums;

namespace Saga.DomainShared.Models;

public class PaginationConfig
{
    public int PageNumber { get; set; } = 1;
    public int[] OptionPageSizes { get; set; } = new int[] { -1, 5, 10, 15, 25, 50, 100 };
    public int PageSize { get; set; } = 15;
    public string? SortBy { get; set; }
    public OrderBy OrderBy { get; set; } = OrderBy.ASC;
    public string? Find { get; set; } = string.Empty;
}
