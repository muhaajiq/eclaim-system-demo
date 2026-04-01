namespace MHA.ECLAIM.Entities.ViewModel.Shared
{
    public class PagedRequest<T>
    {
        public T Search { get; set; }
        public string SortColumn { get; set; } = string.Empty;
        public string SortDirection { get; set; } = "asc";
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
