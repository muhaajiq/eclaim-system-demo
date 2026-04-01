namespace MHA.ECLAIM.Entities.DTO
{
    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public bool IsSuccessful { get; set; } = false;
    }
}
