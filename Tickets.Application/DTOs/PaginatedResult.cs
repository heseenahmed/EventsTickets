namespace FleetLinker.Application.DTOs
{
    public class PaginatedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int? ActiveCount { get; set; } = 0;
        public int? InActiveCount { get; set; } = 0;
        public PaginatedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize , int? activeCount=0, int? inActiveCount=0)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            ActiveCount = activeCount;
            InActiveCount = inActiveCount;
        }
    }
}
