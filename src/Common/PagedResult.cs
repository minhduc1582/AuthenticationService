using System;
using System.Collections.Generic;

namespace Common;

public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyCollection<T> Items { get; set; } = Array.Empty<T>();
}
