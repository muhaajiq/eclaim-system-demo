namespace MHA.ECLAIM.Entities.ViewModel.Shared
{
    public class MyPendingTask
    {
        public string StepName { get; set; }
        public string TaskURL { get; set; }
        public string ProcessID { get; set; }
        public int TaskID { get; set; }
        public string DueDateDisplay { get; set; }
        public bool IsOverDue { get; set; }
        public bool IsUpcomingTask { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string WorkflowDueDate { get; set; }
        public string WorkflowName { get; set; }

        #region Additional Properties
        public string RequestReferenceNo { get; set; }
        public string RequestID { get; set; }
        public string ClaimCategory { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        #endregion
    }
}
