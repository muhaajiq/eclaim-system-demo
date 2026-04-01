namespace MHA.ECLAIM.Entities.ViewModel.Shared
{
    public class Attachment
    {
        public int Id { get; set; }
        public int SubClaimId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string StorageUrl { get; set; } = string.Empty;
    }
}
