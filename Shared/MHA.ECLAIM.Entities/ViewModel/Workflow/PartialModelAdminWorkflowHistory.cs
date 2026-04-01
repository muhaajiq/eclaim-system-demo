namespace MHA.ECLAIM.Entities.ViewModel.Workflow
{
    public class PartialModelAdminWorkflowHistory
    {
        public List<WorkflowHistory> WorkflowHistoryList { get; set; } = new();
        public bool ShowManagement { get; set; } = false;
        public string ProcessID { get; set; } = string.Empty;
        public string TxtNewActioner { get; set; } = string.Empty;
        public bool isWFRunnning { get; set; } = false;
        public string Remark { get; set; } = string.Empty;
        public string WorkflowDueDate { get; set; } = string.Empty;
        //public bool IsCommentCompiler { get; set; }
        //public bool IsCommentApprover { get; set; }
        public bool IsInProgress { get; set; }
        //public int RestrictGroupID { get; set; }

        //public PartialModelAdminWorkflowHistory()
        //{
        //    WorkflowHistoryList = new List<WorkflowHistory>();
        //    ShowManagement = false;
        //    IsWFRunnning = false;
        //}
    }
}
