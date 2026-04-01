namespace MHA.ECLAIM.Entities.ViewModel.Workflow
{
    public class ViewModelMyDelegate
    {
        public bool IsAdmin { get; set; }
        public bool HasItem { get; set; }
        public List<DelegationObject> DelegationList { get; set; }
        public string DateFormat { get; set; }
        public string NewDelegationURL { get; set; }
        public string RemoveDelegationURL { get; set; }
    }
}
