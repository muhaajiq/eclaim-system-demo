using MHA.ECLAIM.Entities.ViewModel.Shared;

namespace MHA.ECLAIM.Entities.ViewModel.Workflow
{
    public class UpdateActionerVM
    {
        public int ProcessID { get; set; }
        public List<PeoplePickerUser> NewActioners { get; set; } = new List<PeoplePickerUser>();
        public PeoplePickerUser CurrentUser { get; set; } = new PeoplePickerUser();
        public bool IsReassign { get; set; }
        public bool AddTaskNow { get; set; }
    }
}
