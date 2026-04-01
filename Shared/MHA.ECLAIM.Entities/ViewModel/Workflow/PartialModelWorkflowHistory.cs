namespace MHA.ECLAIM.Entities.ViewModel.Workflow
{
    public class PartialModelWorkflowHistory
    {
        public List<WorkflowHistory> WorkflowHistoryList { get; set; } = new();
        public string WorkflowDueDate { get; set; } = string.Empty;
        public string ProcessID { get; set; } = string.Empty;
        //public string WorkflowManagementUrl { get; set; } = string.Empty;
        //public string ListName { get; set; } = string.Empty;
        //public string ItemId { get; set; } = string.Empty;
    }
}
