namespace MHA.ECLAIM.Entities.ViewModel.Shared
{
    public class DropDownListItem
    {
        public bool Selected { get; set; } = false;
        public string Text { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string SubText { get; set; } = string.Empty;
        public DropDownListItem() { }
    }
}
