using MHA.ECLAIM.Entities.ViewModel.Shared;

namespace MHA.ECLAIM.Entities.ViewModel.Workflow
{
    public class ReassignActionerVM
    {
        public int ProcessID { get; set; }
        public int TaskID { get; set; }
        public PeoplePickerUser NewActioner { get; set; } = new PeoplePickerUser();
        public string Comments { get; set; } = string.Empty;
    }
}
