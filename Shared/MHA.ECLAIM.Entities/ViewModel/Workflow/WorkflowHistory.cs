namespace MHA.ECLAIM.Entities.ViewModel.Workflow
{
    public class WorkflowHistory
    {
        public string StepName { get; set; } = string.Empty;
        public string TaskURL { get; set; } = string.Empty;
        public int TaskID { get; set; }
        public string DueDate { get; set; } = string.Empty;
        public string AssignedDate { get; set; } = string.Empty;
        public string AssigneeLogin { get; set; } = string.Empty;
        public string AssigneeName { get; set; } = string.Empty;
        public string AssigneeEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ActionedBy { get; set; } = string.Empty;
        public string ActionedByName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string ActionedDate { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public bool ShowRemoveCheckbox { get; set; } = false;
        public bool ShowRemoveLink { get; set; } = false;
        public string ProcessName { get; set; } = string.Empty;
        public string TaskStatus { get; set; } = string.Empty;
        public bool IsChecked { get; set; } = false;
    }
}
