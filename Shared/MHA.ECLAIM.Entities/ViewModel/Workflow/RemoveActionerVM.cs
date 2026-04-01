namespace MHA.ECLAIM.Entities.ViewModel.Workflow
{
    public class RemoveActionerVM
    {
        public int ProcessID { get; set; }
        public int TaskID { get; set; }
        public string WorkflowName { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }
}
