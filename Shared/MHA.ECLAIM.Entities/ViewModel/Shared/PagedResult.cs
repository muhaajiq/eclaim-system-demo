namespace MHA.ECLAIM.Entities.ViewModel.Shared
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public bool IsSuccessful { get; set; }

        public PagedResult(List<T> items, int totalCount, int page, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }

        public PagedResult()
        {
            Items = new List<T>();
            TotalCount = 0;
            Page = 0;
            PageSize = 0;
        }
    }
}
