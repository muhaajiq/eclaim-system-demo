namespace MHA.ECLAIM.Entities.ViewModel.Workflow
{
    public class RemoveAllActionerVM
    {
        public int ProcessID { get; set; }
        public List<int> TaskIDs { get; set; } = new List<int>();
        public List<string> WorkflowNames { get; set; } = new List<string>();
        public string Remark { get; set; }
    }
}
