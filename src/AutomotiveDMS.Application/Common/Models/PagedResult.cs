using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.Common.Models
{
    public class PagedResult<T>
    {
        public List<T> Items { get; init; } = [];
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;

        public static PagedResult<T> Create(
            List<T> items,
            int totalCount,
            int pageNumber,
            int pageSize) =>
            new()
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
    }
}
