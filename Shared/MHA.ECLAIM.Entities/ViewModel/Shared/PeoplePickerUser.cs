namespace MHA.ECLAIM.Entities.ViewModel.Shared
{
    public class PeoplePickerUser
    {
        public int LookupId { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsEmailSendSuccess { get; set; }
        public bool IsThirdParty { get; set; }

        public PeoplePickerUser() { }
    }
}
