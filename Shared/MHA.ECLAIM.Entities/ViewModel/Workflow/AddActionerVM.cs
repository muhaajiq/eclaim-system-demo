using MHA.ECLAIM.Entities.ViewModel.Shared;

namespace MHA.ECLAIM.Entities.ViewModel.Workflow
{
    public class AddActionerVM
    {
        public int ProcessID { get; set; }
        public List<PeoplePickerUser> NewActioner { get; set; } = new List<PeoplePickerUser>();
    }
}
